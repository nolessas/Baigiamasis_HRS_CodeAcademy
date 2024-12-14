class AuthService {
    constructor() {
        this.baseUrl = '/Auth';
        this.token = localStorage.getItem('token');
    }

    async login(username, password) {
        try {
            const response = await fetch(`${this.baseUrl}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();
            console.log('Raw login response:', data);

            if (!response.ok) {
                throw new Error(data.message || 'Login failed');
            }

            const loginData = data.data;
            
            let roles = loginData.roles;
            if (typeof roles === 'string') {
                try {
                    roles = JSON.parse(roles);
                } catch {
                    roles = [roles];
                }
            }
            
            localStorage.setItem('token', loginData.token);
            localStorage.setItem('username', loginData.username);
            localStorage.setItem('roles', JSON.stringify(roles));
            localStorage.setItem('expiresAt', loginData.expiresAt);
            localStorage.setItem('userId', loginData.userId);
            
            this.token = loginData.token;
            return { ...loginData, roles };
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    }

    async signup(username, password) {
        try {
            console.log('Attempting signup with:', { username });

            const response = await fetch(`${this.baseUrl}/signup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            console.log('Server response status:', response.status);
            
            const data = await response.json();
            console.log('Server response data:', data);

            if (!data.isSuccess) {
                throw new Error(data.message || 'Registration failed');
            }

            return data;
        } catch (error) {
            console.error('Signup error details:', error);
            throw error;
        }
    }

    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('username');
        localStorage.removeItem('roles');
        localStorage.removeItem('expiresAt');
        localStorage.removeItem('userId');
        this.token = null;
    }

    isAuthenticated() {
        const token = localStorage.getItem('token');
        const expiresAt = localStorage.getItem('expiresAt');
        if (!token || !expiresAt) return false;
        
        return new Date(expiresAt) > new Date();
    }

    hasRole(role) {
        try {
            const rolesStr = localStorage.getItem('roles');
            if (!rolesStr) return false;
            
            let roles = JSON.parse(rolesStr);
            if (typeof roles[0] === 'string' && roles[0].startsWith('[')) {
                roles = JSON.parse(roles[0]);
            }
            
            console.log('Checking role:', role, 'Available roles:', roles);
            return Array.isArray(roles) && roles.includes(role);
        } catch (error) {
            console.error('Error checking roles:', error);
            return false;
        }
    }
}

const authService = new AuthService(); 