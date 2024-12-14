class AuthService {
    async login(username, password) {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.AUTH.LOGIN, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);

            // Store auth data
            this.#setAuthData(data.data);
            return data.data;
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    }

    async register(username, password) {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.AUTH.REGISTER, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Registration error:', error);
            throw error;
        }
    }

    #setAuthData(authData) {
        localStorage.setItem('token', authData.token);
        localStorage.setItem('userId', authData.userId);
        localStorage.setItem('roles', JSON.stringify(authData.roles));
        localStorage.setItem('expiresAt', authData.expiresAt);
    }
} 