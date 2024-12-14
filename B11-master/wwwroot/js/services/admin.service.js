class AdminService {
    #getAuthHeaders() {
        const token = localStorage.getItem('token');
        return {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };
    }

    async getAllUsers() {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.ADMIN.USERS, {
                headers: this.#getAuthHeaders()
            });

            if (response.status === 403) {
                throw new Error('Access denied. Admin privileges required.');
            }

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Get users error:', error);
            throw error;
        }
    }

    async deleteUser(userId) {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.ADMIN.DELETE_USER(userId), {
                method: 'DELETE',
                headers: this.#getAuthHeaders()
            });

            if (response.status === 403) {
                throw new Error('Access denied. Admin privileges required.');
            }

            if (!response.ok) {
                const data = await response.json();
                throw new Error(data.message);
            }

            return true;
        } catch (error) {
            console.error('Delete user error:', error);
            throw error;
        }
    }
} 