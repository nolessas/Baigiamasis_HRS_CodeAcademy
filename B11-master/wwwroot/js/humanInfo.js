class HumanInfoService {
    constructor() {
        this.baseUrl = '/api/HumanInformation';
    }

    async createHumanInfo(formData) {
        try {
            const response = await fetch(`${this.baseUrl}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to create human information');
            }

            if (response.status === 201) {
                return { isSuccess: true, message: 'Human information created successfully' };
            }

            return await response.json();
        } catch (error) {
            console.error('Create human info error:', error);
            throw error;
        }
    }

    async getHumanInfo(userId) {
        try {
            if (!userId) {
                throw new Error('User ID is required');
            }

            console.log('Fetching user info:', {
                userId,
                url: `${this.baseUrl}/user/${userId}`,
                token: !!localStorage.getItem('token'),
                fullToken: localStorage.getItem('token')
            });

            const response = await fetch(`${this.baseUrl}/user/${userId}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Accept': 'application/json'
                }
            });
            
            const data = await response.json();
            console.log('API Response:', data);
            
            if (response.status === 404) {
                return {
                    isSuccess: false,
                    statusCode: 404,
                    message: 'No user information found. Please complete your profile.',
                    data: null
                };
            }
            
            if (!response.ok) {
                throw new Error(data.message || "Failed to get human information");
            }
            return data;
        } catch (error) {
            console.error('Get human info error:', {
                message: error.message,
                userId: userId,
                apiUrl: this.baseUrl,
                stack: error.stack
            });
            throw error;
        }
    }

    async updateField(id, fieldName, value) {
        try {
            const endpoint = this.getEndpointForField(fieldName);
            const response = await fetch(`${this.baseUrl}/${id}/${endpoint}`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value })
            });

            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || `Failed to update ${fieldName}`);
            }
            return data;
        } catch (error) {
            console.error(`Update ${fieldName} error:`, error);
            throw error;
        }
    }

    getEndpointForField(fieldName) {
        const endpoints = {
            'name': 'name',
            'personalCode': 'personal-code',
            'phone': 'phone',
            'email': 'email',
            'city': 'city',
            'street': 'street',
            'houseNumber': 'house-number',
            'apartmentNumber': 'apartment-number'
        };
        return endpoints[fieldName] || fieldName;
    }

    async updateProfilePicture(id, file) {
        try {
            const formData = new FormData();
            formData.append('profilePicture', file);

            const response = await fetch(`${this.baseUrl}/${id}/profile-picture`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: formData
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update profile picture error:', error);
            throw error;
        }
    }

    async displayUserInfo(containerId) {
        try {
            const userId = localStorage.getItem('userId');
            if (!userId) {
                console.warn('No user ID found in localStorage');
                document.getElementById('humanInfoForm').classList.remove('hidden');
                document.getElementById('userInfoDisplay').classList.add('hidden');
                return;
            }

            const response = await this.getHumanInfo(userId);
            console.log('Human info response:', response);
            
            if (!response.isSuccess || !response.data) {
                console.log('No user information found, showing form');
                document.getElementById('humanInfoForm').classList.remove('hidden');
                document.getElementById('userInfoDisplay').classList.add('hidden');
                
                document.getElementById(containerId).innerHTML = 
                    '<div class="alert alert-info">Please complete your profile information.</div>';
                return;
            }

            console.log('User information found, showing display');
            document.getElementById('humanInfoForm').classList.add('hidden');
            document.getElementById('userInfoDisplay').classList.remove('hidden');
            
            const info = response.data;
            const html = `
                <div class="user-info-container">
                    <div class="profile-section">
                        <div class="profile-image-container">
                            <img src="data:image/jpeg;base64,${info.profilePictureBase64}" alt="Profile" class="profile-image">
                            <div class="image-overlay">
                                <span>Change Photo</span>
                            </div>
                            <input type="file" 
                                   id="profilePictureInput" 
                                   accept="image/*" 
                                   style="display: none;">
                        </div>
                    </div>

                    <div class="info-section">
                        <div class="info-row">
                            <div class="info-label">Name:</div>
                            <div class="info-value" id="nameDisplay">${info.firstName} ${info.lastName}</div>
                            <button class="edit-btn" data-field="name" data-id="${info.id}">Edit</button>
                        </div>

                        <div class="info-row">
                            <div class="info-label">Personal Code:</div>
                            <div class="info-value" id="personalCodeDisplay">${info.personalCode}</div>
                            <button class="edit-btn" data-field="personalCode" data-id="${info.id}">Edit</button>
                        </div>

                        <div class="info-row">
                            <div class="info-label">Phone:</div>
                            <div class="info-value" id="phoneDisplay">${info.phoneNumber}</div>
                            <button class="edit-btn" data-field="phone" data-id="${info.id}">Edit</button>
                        </div>

                        <div class="info-row">
                            <div class="info-label">Email:</div>
                            <div class="info-value" id="emailDisplay">${info.email}</div>
                            <button class="edit-btn" data-field="email" data-id="${info.id}">Edit</button>
                        </div>

                        <div class="address-section">
                            <h3>Address</h3>
                            <div class="info-row">
                                <div class="info-label">City:</div>
                                <div class="info-value" id="cityDisplay">${info.city}</div>
                                <button class="edit-btn" data-field="city" data-id="${info.id}">Edit</button>
                            </div>

                            <div class="info-row">
                                <div class="info-label">Street:</div>
                                <div class="info-value" id="streetDisplay">${info.street}</div>
                                <button class="edit-btn" data-field="street" data-id="${info.id}">Edit</button>
                            </div>

                            <div class="info-row">
                                <div class="info-label">House Number:</div>
                                <div class="info-value" id="houseNumberDisplay">${info.houseNumber}</div>
                                <button class="edit-btn" data-field="houseNumber" data-id="${info.id}">Edit</button>
                            </div>

                            <div class="info-row">
                                <div class="info-label">Apartment Number:</div>
                                <div class="info-value" id="apartmentNumberDisplay">${info.apartmentNumber || '-'}</div>
                                <button class="edit-btn" data-field="apartmentNumber" data-id="${info.id}">Edit</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            document.getElementById(containerId).innerHTML = html;

            // Add event listeners after rendering
            this.initializeEventListeners(info.id);
        } catch (error) {
            console.error('Display user info error:', {
                message: error.message,
                stack: error.stack
            });
            
            document.getElementById('humanInfoForm').classList.remove('hidden');
            document.getElementById('userInfoDisplay').classList.add('hidden');
            document.getElementById(containerId).innerHTML = 
                '<div class="alert alert-danger">Error loading user information. Please try creating your profile.</div>';
        }
    }

    initializeEventListeners(id) {
        // Add click handler for profile picture change
        const profileImageContainer = document.querySelector('.profile-image-container');
        const profilePictureInput = document.getElementById('profilePictureInput');
        
        if (profileImageContainer && profilePictureInput) {
            // Remove existing listeners before adding new ones
            profileImageContainer.replaceWith(profileImageContainer.cloneNode(true));
            const newProfileImageContainer = document.querySelector('.profile-image-container');
            const newProfilePictureInput = document.getElementById('profilePictureInput');
            
            newProfileImageContainer.addEventListener('click', () => {
                newProfilePictureInput.click();
            });

            newProfilePictureInput.addEventListener('change', (event) => {
                handleProfilePictureChange(event, id);
            });
        }

        // Remove existing edit button listeners and add new ones
        document.querySelectorAll('.edit-btn').forEach(button => {
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);
            
            newButton.addEventListener('click', (e) => {
                // Remove any existing modals before creating a new one
                const existingModals = document.querySelectorAll('.modal');
                existingModals.forEach(modal => modal.remove());
                
                const field = e.target.dataset.field;
                const id = e.target.dataset.id;
                editField(field, id);
            });
        });
    }

    async updateName(id, firstName, lastName) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/name`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ firstName, lastName })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update name error:', error);
            throw error;
        }
    }

    async updateEmail(id, email) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/email`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: email })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update email error:', error);
            throw error;
        }
    }

    async updatePersonalCode(id, personalCode) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/personal-code`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: personalCode })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update personal code error:', error);
            throw error;
        }
    }

    async updatePhone(id, phone) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/phone`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: phone })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update phone error:', error);
            throw error;
        }
    }

    async updateCity(id, city) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/city`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: city })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update city error:', error);
            throw error;
        }
    }

    async updateStreet(id, street) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/street`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: street })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update street error:', error);
            throw error;
        }
    }

    async updateHouseNumber(id, houseNumber) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/house-number`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: houseNumber })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update house number error:', error);
            throw error;
        }
    }

    async updateApartmentNumber(id, apartmentNumber) {
        try {
            const response = await fetch(`${this.baseUrl}/${id}/apartment-number`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ value: apartmentNumber })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update apartment number error:', error);
            throw error;
        }
    }

    async updateProfilePicture(id, file) {
        try {
            const formData = new FormData();
            formData.append('profilePicture', file);

            const response = await fetch(`${this.baseUrl}/${id}/profile-picture`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: formData
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Update profile picture error:', error);
            throw error;
        }
    }
}

const humanInfoService = new HumanInfoService(); 

async function handleHumanInfoSubmit(e) {
    e.preventDefault();
    
    // Add client-side validation
    const validation = Validator.validateForm(e.target);
    if (!validation.isValid) {
        Object.entries(validation.errors).forEach(([field, message]) => {
            const input = e.target.querySelector(`[name="${field}"]`);
            const errorDiv = input?.nextElementSibling;
            if (errorDiv) {
                errorDiv.textContent = message;
            }
        });
        return;
    }

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

// Replace the editField function with this version
window.editField = async function(fieldName, id) {
    try {
        // Remove any existing modals first
        const existingModals = document.querySelectorAll('.modal');
        existingModals.forEach(modal => modal.remove());

        let currentValue = document.getElementById(`${fieldName}Display`).textContent;
        
        // Create modal with proper styling
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.style.display = 'flex';

        // Define validation patterns and placeholders based on field type
        let validationPattern = '';
        let placeholder = '';
        let inputType = 'text';

        switch(fieldName) {
            case 'name':
                const [firstName, lastName] = currentValue.split(' ');
                inputHtml = `
                    <div class="form-field">
                        <label>First Name</label>
                        <input type="text" 
                               name="firstName" 
                               value="${firstName}"
                               pattern="[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž]+" 
                               title="Must start with uppercase and contain only Lithuanian letters"
                               required>
                        <div class="error-message"></div>
                    </div>
                    <div class="form-field">
                        <label>Last Name</label>
                        <input type="text" 
                               name="lastName" 
                               value="${lastName}"
                               pattern="[A-ZĄČĘĮŠŲŪŽ][a-ząčęėįšųūž]+" 
                               title="Must start with uppercase and contain only Lithuanian letters"
                               required>
                        <div class="error-message"></div>
                    </div>`;
                break;
            case 'personalCode':
                validationPattern = '[3-6]\\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\\d|3[01])\\d{4}';
                placeholder = "Personal Code (e.g., 39001011234)";
                title = "Valid Lithuanian personal code format";
                break;
            case 'phone':
                validationPattern = '\\+370\\d{8}';
                placeholder = "+370xxxxxxxx";
                title = "Format: +370xxxxxxxx";
                break;
            case 'email':
                inputType = 'email';
                validationPattern = '[a-zA-Z0-9._%+-]+@(gmail\\.com|yahoo\\.com)';
                placeholder = "email@gmail.com";
                title = "Only gmail.com and yahoo.com domains are allowed";
                break;
            case 'city':
            case 'street':
                validationPattern = '[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž\\s-]+';
                placeholder = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
                title = "Must start with uppercase and contain only letters, spaces and hyphens";
                break;
            case 'houseNumber':
                validationPattern = '[1-9]\\d{0,3}[A-Za-z]?';
                placeholder = "House number (e.g., 123A)";
                title = "Number (1-9999) optionally followed by a letter";
                break;
            case 'apartmentNumber':
                validationPattern = '[1-9]\\d{0,3}|^$';
                placeholder = "Apartment number (optional)";
                title = "Positive number or leave empty";
                break;
        }

        // Create input HTML for non-name fields
        if (fieldName !== 'name') {
            inputHtml = `
                <div class="form-field">
                    <label>${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)}</label>
                    <input type="${inputType}" 
                           name="${fieldName}" 
                           value="${currentValue}"
                           pattern="${validationPattern}"
                           placeholder="${placeholder}"
                           title="${title}"
                           required>
                    <div class="error-message"></div>
                </div>`;
        }

        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Edit ${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)}</h3>
                    <button type="button" class="close-btn" onclick="this.closest('.modal').remove()">&times;</button>
                </div>
                <form id="editForm">
                    ${inputHtml}
                    <div class="modal-buttons">
                        <button type="button" class="cancel-btn" onclick="this.closest('.modal').remove()">Cancel</button>
                        <button type="submit" class="submit-btn">Save</button>
                    </div>
                </form>
            </div>
        `;

        document.body.appendChild(modal);

        // Add focus to the first input
        setTimeout(() => {
            const firstInput = modal.querySelector('input');
            if (firstInput) firstInput.focus();
        }, 100);

        // Handle form submission
        const form = modal.querySelector('form');
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            let newValue;
            if (fieldName === 'name') {
                const firstName = form.querySelector('input[name="firstName"]').value;
                const lastName = form.querySelector('input[name="lastName"]').value;
                
                // Validate both first name and last name
                const firstNameValidation = Validator.validateField('firstname', firstName);
                const lastNameValidation = Validator.validateField('lastname', lastName);
                
                if (!firstNameValidation.isValid) {
                    form.querySelector('input[name="firstName"]').nextElementSibling.textContent = firstNameValidation.message;
                    return;
                }
                if (!lastNameValidation.isValid) {
                    form.querySelector('input[name="lastName"]').nextElementSibling.textContent = lastNameValidation.message;
                    return;
                }
                
                newValue = `${firstName} ${lastName}`;
            } else {
                newValue = form.querySelector(`input[name="${fieldName}"]`).value;
                
                // Validate the field
                const validation = Validator.validateField(fieldName.toLowerCase(), newValue);
                if (!validation.isValid) {
                    form.querySelector('.error-message').textContent = validation.message;
                    return;
                }
            }

            try {
                const result = await humanInfoService.updateField(id, fieldName, newValue);
                
                if (result.isSuccess) {
                    modal.remove();
                    showMessage(`${fieldName} updated successfully`, 'success');
                    await loadHumanInformation(); // Refresh the display
                } else {
                    showMessage(result.message || `Failed to update ${fieldName}`, 'error');
                }
            } catch (error) {
                console.error(`Error updating ${fieldName}:`, error);
                showMessage(error.message || `Failed to update ${fieldName}`, 'error');
            }
        });

        // Add real-time validation feedback
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.addEventListener('input', function() {
                const fieldName = this.name.toLowerCase();
                const validation = Validator.validateField(fieldName, this.value);
                const errorDiv = this.nextElementSibling;
                
                if (errorDiv && errorDiv.classList.contains('error-message')) {
                    errorDiv.textContent = validation.isValid ? '' : validation.message;
                }
                
                this.classList.toggle('invalid', !validation.isValid);
            });
        });

        // Close modal with ESC key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                modal.remove();
            }
        });

        // Close modal when clicking outside
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                modal.remove();
            }
        });

    } catch (error) {
        console.error('Edit field error:', error);
        showMessage('Failed to edit field', 'error');
    }
}

// Add this function to handle profile picture updates
window.handleProfilePictureChange = async function(event, id) {
    const file = event.target.files[0];
    if (!file) return;

    try {
        const validation = Validator.validateImage(file);
        if (!validation.isValid) {
            showMessage(validation.message, 'error');
            return;
        }

        const formData = new FormData();
        formData.append('profilePicture', file);

        const result = await humanInfoService.updateProfilePicture(id, file);
        
        if (result.isSuccess) {
            showMessage('Profile picture updated successfully', 'success');
            await loadHumanInformation(); // Refresh the display
        } else {
            showMessage(result.message || 'Failed to update profile picture', 'error');
        }
    } catch (error) {
        console.error('Error updating profile picture:', error);
        showMessage(error.message || 'Failed to update profile picture', 'error');
    }
}