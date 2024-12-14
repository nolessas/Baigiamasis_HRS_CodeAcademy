class AdminService {
    constructor() {
        this.baseUrl = '/api/Admin';
    }

    getAuthHeaders() {
        const token = localStorage.getItem('token');
        if (!token) {
            throw new Error('No authentication token found');
        }
        return {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
    }

    async getAllUsers() {
        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('No authentication token found');
            }

            const response = await fetch(`${this.baseUrl}/users`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            });

            if (response.status === 403) {
                throw new Error('Access denied. You do not have permission to view users.');
            }

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to fetch users');
            }

            const data = await response.json();
            console.log('Fetched users:', data);
            
            if (!data.isSuccess) {
                throw new Error(data.message || 'Failed to fetch users');
            }

            return data;
        } catch (error) {
            console.error('Error fetching users:', error);
            throw error;
        }
    }

    async deleteUser(userId) {
        try {
            const response = await fetch(`${this.baseUrl}/users/${userId}`, {
                method: 'DELETE',
                headers: this.getAuthHeaders()
            });

            if (response.status === 403) {
                throw new Error('Access denied. You do not have permission to delete users.');
            }

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to delete user');
            }

            return true;
        } catch (error) {
            console.error('Error deleting user:', error);
            throw error;
        }
    }

    async testAuth() {
        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('No authentication token found');
            }

            // Decode the JWT token
            const payload = JSON.parse(atob(token.split('.')[1]));
            const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            
            // Check if roles contains 'Admin'
            const isAdmin = Array.isArray(roles) 
                ? roles.includes('Admin')
                : roles === 'Admin';

            return isAdmin;
        } catch (error) {
            console.error('Auth verification failed:', error);
            throw new Error('Authentication failed');
        }
    }
}

const adminService = new AdminService();

async function openAdminPanel() {
    try {
        const isAdmin = await adminService.testAuth();
        if (!isAdmin) {
            showMessage('Access denied. Admin privileges required.', 'error');
            return;
        }
        
        const modal = document.getElementById('adminPanelModal');
        if (!modal) {
            console.error('Admin panel modal not found');
            return;
        }
        
        modal.classList.remove('hidden');
        await loadUsers();
    } catch (error) {
        console.error('Failed to open admin panel:', error);
        showMessage('Access denied. Please check your permissions.', 'error');
    }
}

function closeAdminPanel() {
    const modal = document.getElementById('adminPanelModal');
    if (modal) {
        modal.classList.add('hidden');
    }
}

function createUserCard(user) {
    return `
        <div class="user-card" data-user-id="${user.id}">
            <div class="user-info">
                <h3>${user.username}</h3>
                <p>Role: ${user.roles}</p>
            </div>
            ${!user.roles.includes('Admin') ? 
                `<button onclick="deleteUser('${user.id}')" class="delete-btn">Delete</button>` : 
                '<span class="admin-badge">Admin</span>'
            }
        </div>
    `;
}

async function loadUsers() {
    const usersList = document.getElementById('adminUsersList');
    if (!usersList) {
        console.error('Users list container not found');
        return;
    }

    try {
        usersList.innerHTML = '<div class="loading">Loading users...</div>';
        const response = await adminService.getAllUsers();
        
        if (response.isSuccess && Array.isArray(response.data) && response.data.length > 0) {
            usersList.innerHTML = response.data.map(user => `
                <div class="user-card" data-user-id="${user.id}">
                    <div class="user-info">
                        <h3>${user.username}</h3>
                        <div class="user-details">
                            <p>Created: ${new Date(user.createdDate).toLocaleDateString()}</p>
                            <p>Role: ${user.roles.join(', ')}</p>
                            <p>Has Profile: ${user.hasHumanInformation ? 'Yes' : 'No'}</p>
                        </div>
                    </div>
                    <div class="user-actions">
                        ${!user.roles.includes('Admin') ? 
                            `<button onclick="deleteUser('${user.id}')" class="delete-btn" title="Delete User">&times;</button>` : 
                            '<span class="admin-badge">Admin</span>'
                        }
                    </div>
                </div>
            `).join('');
        } else {
            usersList.innerHTML = '<div class="no-users">No users found</div>';
        }
    } catch (error) {
        console.error('Error loading users:', error);
        usersList.innerHTML = `<div class="error">Error: ${error.message}</div>`;
    }
}

async function deleteUser(userId) {
    if (!confirm('Are you sure you want to delete this user?')) {
        return;
    }

    const userCard = document.querySelector(`[data-user-id="${userId}"]`);
    try {
        if (userCard) {
            userCard.style.opacity = '0.5';
            userCard.style.pointerEvents = 'none';
        }

        const response = await adminService.deleteUser(userId);
        if (response) {
            await loadUsers(); // Reload the list after successful deletion
            showMessage('User deleted successfully', 'success');
        }
    } catch (error) {
        if (userCard) {
            userCard.style.opacity = '1';
            userCard.style.pointerEvents = 'auto';
        }
        showMessage(error.message || 'Failed to delete user', 'error');
    }
}

function filterUsers() {
    const adminSearchTerm = document.getElementById('adminUserSearch')?.value.toLowerCase() || '';
    const modalSearchTerm = document.getElementById('modalUserSearch')?.value.toLowerCase() || '';
    const searchTerm = adminSearchTerm || modalSearchTerm;

    const userCards = document.querySelectorAll('.user-card');
    userCards.forEach(card => {
        const username = card.querySelector('h3').textContent.toLowerCase();
        card.style.display = username.includes(searchTerm) ? '' : 'none';
    });
}

// Initialize admin panel when the page loads
document.addEventListener('DOMContentLoaded', () => {
    const adminPanelBtn = document.getElementById('adminPanelBtn');
    if (adminPanelBtn) {
        adminPanelBtn.addEventListener('click', openAdminPanel);
    }

    const adminSearch = document.getElementById('adminUserSearch');
    const modalSearch = document.getElementById('modalUserSearch');
    
    if (adminSearch) {
        adminSearch.addEventListener('input', filterUsers);
    }
    if (modalSearch) {
        modalSearch.addEventListener('input', filterUsers);
    }
}); 