class Validator {
    static patterns = {
        username: /^[a-zA-Z0-9._-]{3,50}$/,
        password: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/,
        firstname: /^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž]+$/,
        lastname: /^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž]+$/,
        personalcode: /^[3-6]\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{4}$/,
        phonenumber: /^\+370\d{8}$/,
        email: /^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com)$/,
        city: /^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž\s-]+$/,
        street: /^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž\s-]+$/,
        housenumber: /^[1-9]\d{0,3}[A-Za-z]?$/,
        apartmentnumber: /^[1-9]\d{0,3}$|^$/,
    };

    static messages = {
        username: "Username must be 3-50 characters long and can contain letters, numbers, dots, underscores and hyphens",
        password: "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, and one number",
        firstname: "First name must start with uppercase and contain only Lithuanian letters",
        lastname: "Last name must start with uppercase and contain only Lithuanian letters", 
        personalcode: "Invalid Lithuanian personal code format (11 digits starting with 3-6)",
        phonenumber: "Phone number must be in format: +370xxxxxxxx",
        email: "Invalid email format (example@domain.com)",
        city: "City must start with uppercase and contain only letters, spaces and hyphens",
        street: "Street must start with uppercase and contain only letters, spaces and hyphens",
        housenumber: "House number must be a number (1-9999) optionally followed by a letter",
        apartmentnumber: "Apartment number must be a positive number or empty",
    };

    static validateField(fieldName, value) {
        const normalizedFieldName = fieldName.toLowerCase().replace('.', '');
        
        if (!value && normalizedFieldName !== 'apartmentnumber') {
            return {
                isValid: false,
                message: `${this.getFieldLabel(normalizedFieldName)} is required`
            };
        }

        if (normalizedFieldName === 'apartmentnumber' && !value) {
            return { isValid: true };
        }

        const pattern = this.patterns[normalizedFieldName];
        if (!pattern) {
            return { isValid: true };
        }

        const isValid = pattern.test(value);
        return {
            isValid,
            message: isValid ? null : this.messages[normalizedFieldName]
        };
    }

    static validateImage(file) {
        if (!file) {
            return { isValid: false, message: "Profile picture is required" };
        }

        const validTypes = ['image/jpeg', 'image/png', 'image/gif'];
        const maxSize = 5 * 1024 * 1024; // 5MB

        if (!validTypes.includes(file.type)) {
            return { isValid: false, message: "Only JPEG, PNG and GIF files are allowed" };
        }

        if (file.size > maxSize) {
            return { isValid: false, message: "File size must be less than 5MB" };
        }

        return { isValid: true };
    }

    static validateForm(formElement) {
        let isValid = true;
        const errors = {};

        const inputs = formElement.querySelectorAll('input');
        
        inputs.forEach(input => {
            const fieldName = input.name.toLowerCase().replace('.', '');
            const validation = this.validateField(fieldName, input.value);
            
            if (!validation.isValid) {
                isValid = false;
                errors[fieldName] = validation.message;
                
                input.classList.add('invalid');
                
                const errorDiv = input.nextElementSibling;
                if (errorDiv && errorDiv.classList.contains('error-message')) {
                    errorDiv.textContent = validation.message;
                }
            } else {
                input.classList.remove('invalid');
                const errorDiv = input.nextElementSibling;
                if (errorDiv && errorDiv.classList.contains('error-message')) {
                    errorDiv.textContent = '';
                }
            }
        });

        return { isValid, errors };
    }

    static getFieldLabel(fieldName) {
        const labels = {
            firstname: 'First Name',
            lastname: 'Last Name',
            personalcode: 'Personal Code',
            phonenumber: 'Phone Number',
            email: 'Email',
            city: 'City',
            street: 'Street',
            housenumber: 'House Number',
            apartmentnumber: 'Apartment Number'
        };
        return labels[fieldName] || fieldName;
    }
}