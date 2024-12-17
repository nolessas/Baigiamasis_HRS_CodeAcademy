document.addEventListener('DOMContentLoaded', () => {
    // Initialize UI elements
    initializeUIElements();
    
    // Check authentication status
    updateUIBasedOnAuth();
    
    // Initialize form validation
    initializeFormValidation();
    
    // Initialize human info service if authenticated
    if (authService.isAuthenticated()) {
        const humanInfo = new HumanInfoService();
        humanInfo.displayUserInfo('userInfoContent');
    }
});

function initializeUIElements() {
    const loginBtn = document.getElementById('loginBtn');
    const registerBtn = document.getElementById('registerBtn');
    const logoutBtn = document.getElementById('logoutBtn');
    
    if (loginBtn) loginBtn.addEventListener('click', () => showForm('login'));
    if (registerBtn) registerBtn.addEventListener('click', () => showForm('register'));
    if (logoutBtn) logoutBtn.addEventListener('click', handleLogout);
    
    // Initialize form submissions
    const loginForm = document.getElementById('loginFormElement');
    const registerForm = document.getElementById('registerFormElement');
    const humanInfoForm = document.getElementById('humanInfoFormElement');
    
    if (loginForm) loginForm.addEventListener('submit', handleLogin);
    if (registerForm) registerForm.addEventListener('submit', handleRegister);
    if (humanInfoForm) humanInfoForm.addEventListener('submit', handleHumanInfoSubmit);
}

function updateUIBasedOnAuth() {
    const isAuth = authService.isAuthenticated();
    const isAdmin = authService.hasRole('Admin');
    
    const loginBtn = document.getElementById('loginBtn');
    const registerBtn = document.getElementById('registerBtn');
    const logoutBtn = document.getElementById('logoutBtn');
    const adminPanelBtn = document.getElementById('adminPanelBtn');
    const authForms = document.getElementById('authForms');
    
    if (isAuth) {
        // Hide auth buttons and forms
        loginBtn.classList.add('hidden');
        registerBtn.classList.add('hidden');
        authForms.classList.add('hidden');
        
        // Show logout button
        logoutBtn.classList.remove('hidden');
        
        if (isAdmin) {
            adminPanelBtn.classList.remove('hidden');
            // Reinitialize admin panel
            initializeAdminPanel();
        } else {
            adminPanelBtn.classList.add('hidden');
        }
        
        loadHumanInformation();
    } else {
        // Show auth buttons and forms
        loginBtn.classList.remove('hidden');
        registerBtn.classList.remove('hidden');
        authForms.classList.remove('hidden');
        
        // Hide logged-in user elements
        logoutBtn.classList.add('hidden');
        adminPanelBtn.classList.add('hidden');
    }
}

function showForm(type) {
    // Hide both forms first
    document.getElementById('loginForm').classList.add('hidden');
    document.getElementById('registerForm').classList.add('hidden');
    
    // Show the requested form
    document.getElementById(`${type}Form`).classList.remove('hidden');
}

async function loadHumanInformation() {
    try {
        const userId = localStorage.getItem('userId');
        if (!userId) {
            document.getElementById('humanInfoForm').classList.remove('hidden');
            document.getElementById('userInfoDisplay').classList.add('hidden');
            return;
        }

        const humanInfo = new HumanInfoService();
        const response = await humanInfo.getHumanInfo(userId);
        
        // Handle case where user hasn't submitted info yet
        if (!response.isSuccess || !response.data) {
            // Show the form for new users
            document.getElementById('humanInfoForm').classList.remove('hidden');
            document.getElementById('userInfoDisplay').classList.add('hidden');
            
            // Only show message if it's a new user (404)
            if (response.statusCode === 404) {
                showMessage('Please complete your profile information', 'info');
            }
            return;
        }

        // User has data, show the display
        document.getElementById('humanInfoForm').classList.add('hidden');
        document.getElementById('userInfoDisplay').classList.remove('hidden');
        await humanInfo.displayUserInfo('userInfoContent');
    } catch (error) {
        // Only show error for unexpected issues
        if (!error.message.includes('Please complete your profile')) {
            console.error('Error loading human information:', error.message);
            showMessage('Error loading profile information', 'error');
        }
        
        document.getElementById('humanInfoForm').classList.remove('hidden');
        document.getElementById('userInfoDisplay').classList.add('hidden');
    }
}

function displayHumanInformation(info) {
    const content = document.getElementById('userInfoContent');
    content.innerHTML = `
        <div class="info-group">
            <h3>Personal Information</h3>
            <p><strong>Name:</strong> ${info.firstName} ${info.lastName}</p>
            <p><strong>Personal Code:</strong> ${info.personalCode}</p>
            <p><strong>Phone:</strong> ${info.phoneNumber}</p>
            <p><strong>Email:</strong> ${info.email}</p>
        </div>
        <div class="info-group">
            <h3>Address</h3>
            <p><strong>City:</strong> ${info.address.city}</p>
            <p><strong>Street:</strong> ${info.address.street}</p>
            <p><strong>House Number:</strong> ${info.address.houseNumber}</p>
            ${info.address.apartmentNumber ? `<p><strong>Apartment:</strong> ${info.address.apartmentNumber}</p>` : ''}
        </div>
        <div class="profile-picture">
            <h3>Profile Picture</h3>
            <img src="data:image/jpeg;base64,${info.profilePicture}" alt="Profile Picture">
        </div>
    `;
}

async function handleLogin(e) {
    e.preventDefault();
    
    const validation = Validator.validateForm(e.target);

    if (!validation.isValid) {
        // Show specific error messages
        Object.entries(validation.errors).forEach(([field, message]) => {
            const input = e.target.querySelector(`input[name="${field}"]`);
            const errorDiv = input?.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('error-message')) {
                errorDiv.textContent = message;
            }
        });
        showMessage('Please correct the errors in the form', 'error');
        return;
    }

    const username = e.target.querySelector('input[name="username"]').value;
    const password = e.target.querySelector('input[name="password"]').value;

    try {
        const response = await authService.login(username, password);
        if (response) {
            showMessage('Login successful!', 'success');
            checkAuthState(); // Log auth state after login
            updateUIBasedOnAuth();
            e.target.reset();
        }
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

let isSubmitting = false;

async function handleRegister(e) {
    e.preventDefault();
    
    const validation = Validator.validateForm(e.target);
    if (!validation.isValid) {
        // Show specific error messages
        Object.entries(validation.errors).forEach(([field, message]) => {
            const input = e.target.querySelector(`input[name="${field}"]`);
            const errorDiv = input?.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('error-message')) {
                errorDiv.textContent = message;
            }
        });
        showMessage('Please correct the errors in the form', 'error');
        return;
    }

    const username = e.target.querySelector('input[name="username"]').value;
    const password = e.target.querySelector('input[name="password"]').value;

    try {
        const response = await authService.signup(username, password);
        if (response.isSuccess) {
            showMessage('Registration successful! Please login.', 'success');
            showForm('login');
            e.target.reset();
        } else {
            showMessage(response.message || 'Registration failed', 'error');
        }
    } catch (error) {
        // Handle specific error messages
        const errorMessage = error.message.includes('Username already taken') 
            ? 'This username is already taken. Please choose another one.'
            : error.message || 'An error occurred during registration';
            
        showMessage(errorMessage, 'error');
        console.error('Registration error:', error);
    }
}

async function handleHumanInfoSubmit(e) {
    e.preventDefault();
    
    const validation = Validator.validateForm(e.target);
    if (!validation.isValid) {
        showMessage('Please correct the errors in the form', 'error');
        return;
    }

    // Continue with form submission...
    const formData = new FormData(e.target);
    try {
        const response = await humanInfoService.createHumanInfo(formData);
        if (response.isSuccess) {
            showMessage('Information saved successfully!', 'success');
            await loadHumanInformation();
        } else {
            showMessage(response.message || 'Failed to save information', 'error');
        }
    } catch (error) {
        showMessage(error.message || 'An error occurred', 'error');
    }
}

function handleLogout() {
    authService.logout();
    // Reset forms to initial state
    document.getElementById('loginFormElement').reset();
    document.getElementById('registerFormElement').reset();
    document.getElementById('humanInfoFormElement').reset();
    
    // Show auth forms and hide user info
    document.getElementById('authForms').classList.remove('hidden');
    document.getElementById('humanInfoForm').classList.add('hidden');
    document.getElementById('userInfoDisplay').classList.add('hidden');
    
    // Update UI
    updateUIBasedOnAuth();
    showMessage('Logged out successfully!', 'success');
}
function showMessage(message, type = 'success') {
    // Remove any existing notification
    const existingNotification = document.querySelector('.notification-bar');
    if (existingNotification) {
        existingNotification.remove();
    }

    // Create new notification
    const notification = document.createElement('div');
    notification.className = `notification-bar ${type}`;
    notification.textContent = message;

    // Add to document
    document.body.appendChild(notification);

    // Force reflow to enable animation
    notification.offsetHeight;

    // Show notification
    notification.classList.add('show');

    // Hide after delay
    setTimeout(() => {
        notification.classList.add('hide');
        setTimeout(() => notification.remove(), 300); // Remove after hide animation
    }, 3000);
}

// Add function to handle admin features
function showAdminFeatures() {
    const adminSection = document.querySelector('.admin-section');
    if (adminSection) {
        adminSection.classList.remove('hidden');
    }
}

// Update handlers
async function handleUpdate(fieldName) {
    const modal = createUpdateModal(fieldName);
    document.body.appendChild(modal);
    modal.style.display = 'block';

    const form = modal.querySelector('form');
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const value = form.querySelector('input').value;
        const userId = localStorage.getItem('userId');

        try {
            await humanInfoService.updateField(userId, fieldName, value);
            await humanInfoService.displayUserInfo('userInfoContent');
            modal.remove();
            showMessage(`${fieldName} updated successfully`, 'success');
            reattachAdminPanelListener();
        } catch (error) {
            showMessage(error.message, 'error');
        }
    });
}

async function handleImageUpdate() {
    const modal = createImageUpdateModal();
    document.body.appendChild(modal);
    modal.style.display = 'block';

    const form = modal.querySelector('form');
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const imageFile = form.querySelector('input[type="file"]').files[0];
        const userId = localStorage.getItem('userId');

        try {
            await humanInfoService.updateProfilePicture(userId, imageFile);
            await humanInfoService.displayUserInfo('userInfoContent');
            modal.remove();
            showMessage('Profile picture updated successfully', 'success');
            reattachAdminPanelListener();
        } catch (error) {
            showMessage(error.message, 'error');
        }
    });
}

async function handleAddressUpdate() {
    const modal = createAddressUpdateModal();
    document.body.appendChild(modal);
    modal.style.display = 'block';

    const form = modal.querySelector('form');
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const userId = localStorage.getItem('userId');
        const address = {
            city: form.querySelector('[name="city"]').value,
            street: form.querySelector('[name="street"]').value,
            houseNumber: form.querySelector('[name="houseNumber"]').value,
            apartmentNumber: form.querySelector('[name="apartmentNumber"]').value
        };

        try {
            await humanInfoService.updateField(userId, 'address', address);
            await humanInfoService.displayUserInfo('userInfoContent');
            modal.remove();
            showMessage('Address updated successfully', 'success');
            reattachAdminPanelListener();
        } catch (error) {
            showMessage(error.message, 'error');
        }
    });
}

// Modal creators
function createUpdateModal(fieldName) {
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-content">
            <h3>Update ${fieldName}</h3>
            <form>
                <input type="text" name="${fieldName}" required placeholder="Enter new ${fieldName}">
                <div class="modal-buttons">
                    <button type="button" onclick="this.closest('.modal').remove()">Cancel</button>
                    <button type="submit">Update</button>
                </div>
            </form>
        </div>
    `;
    return modal;
}

function createImageUpdateModal() {
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-content">
            <h3>Update Profile Picture</h3>
            <form>
                <input type="file" accept="image/*" required>
                <div class="modal-buttons">
                    <button type="button" onclick="this.closest('.modal').remove()">Cancel</button>
                    <button type="submit">Update</button>
                </div>
            </form>
        </div>
    `;
    return modal;
}

function createAddressUpdateModal() {
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.innerHTML = `
        <div class="modal-content">
            <h3>Update Address</h3>
            <form>
                <input type="text" name="city" required placeholder="City">
                <input type="text" name="street" required placeholder="Street">
                <input type="text" name="houseNumber" required placeholder="House Number">
                <input type="text" name="apartmentNumber" placeholder="Apartment Number">
                <div class="modal-buttons">
                    <button type="button" onclick="this.closest('.modal').remove()">Cancel</button>
                    <button type="submit">Update</button>
                </div>
            </form>
        </div>
    `;
    return modal;
}

// Initialize user info display after login
document.addEventListener('DOMContentLoaded', () => {
    if (authService.isAuthenticated()) {
        humanInfoService.displayUserInfo('userInfoContent');
    }
});

async function handleProfilePictureChange(event, id) {
    const file = event.target.files[0];
    if (!file) return;

    try {
        const validation = Validator.validateImage(file);
        if (!validation.isValid) {
            showMessage(validation.message, 'error');
            return;
        }

        const result = await humanInfoService.updateProfilePicture(id, file);
        
        if (result.isSuccess) {
            showMessage('Profile picture updated successfully', 'success');
            await loadHumanInformation();
        } else {
            showMessage(result.message || 'Failed to update profile picture', 'error');
        }
    } catch (error) {
        console.error('Error updating profile picture:', error);
        showMessage(error.message || 'Failed to update profile picture', 'error');
    }
}

function initializeFormValidation() {
    // Add input event listeners for real-time validation
    document.querySelectorAll('input').forEach(input => {
        input.addEventListener('input', function() {
            const validation = Validator.validateField(this.name, this.value);
            const errorDiv = this.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('error-message')) {
                errorDiv.textContent = validation.isValid ? '' : validation.message;
            }
            this.classList.toggle('invalid', !validation.isValid);
        });
    });

    // Add password strength indicator
    const passwordInputs = document.querySelectorAll('input[type="password"]');
    passwordInputs.forEach(input => {
        input.addEventListener('input', function() {
            const validation = Validator.validatePassword(this.value);
            const strengthDiv = this.nextElementSibling;
            if (strengthDiv && strengthDiv.classList.contains('password-strength')) {
                strengthDiv.className = 'password-strength ' + 
                    (validation.score <= 2 ? 'weak' : 
                     validation.score <= 3 ? 'fair' : 'strong');
            }
        });
    });
}

// Add this debug function
function checkAuthState() {
    const state = {
        userId: localStorage.getItem('userId'),
        hasToken: !!localStorage.getItem('token'),
        token: localStorage.getItem('token')?.substring(0, 20) + '...',
        roles: localStorage.getItem('roles'),
        isAuthenticated: authService.isAuthenticated()
    };
    console.log('Auth State:', state);
    return state;
}

function reattachAdminPanelListener() {
    const adminPanelBtn = document.getElementById('adminPanelBtn');
    if (adminPanelBtn) {
        adminPanelBtn.removeEventListener('click', openAdminPanel);
        adminPanelBtn.addEventListener('click', openAdminPanel);
    }
}