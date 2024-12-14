class HumanInfoService {
    #getAuthHeaders() {
        const token = localStorage.getItem('token');
        return {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };
    }

    async getUserInfo(userId) {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.HUMAN_INFO.GET_BY_USER(userId), {
                headers: this.#getAuthHeaders()
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error('Get user info error:', error);
            throw error;
        }
    }

    async updateField(id, field, value) {
        try {
            const response = await fetch(ApiConfig.ENDPOINTS.HUMAN_INFO.UPDATE_FIELD(id, field), {
                method: 'PATCH',
                headers: this.#getAuthHeaders(),
                body: JSON.stringify({ value })
            });

            const data = await response.json();
            if (!response.ok) throw new Error(data.message);
            return data;
        } catch (error) {
            console.error(`Update ${field} error:`, error);
            throw error;
        }
    }

    async updateProfilePicture(id, file) {
        try {
            const formData = new FormData();
            formData.append('profilePicture', file);

            const headers = this.#getAuthHeaders();
            delete headers['Content-Type']; // Let browser set correct content type for FormData

            const response = await fetch(ApiConfig.ENDPOINTS.HUMAN_INFO.UPDATE_PROFILE_PICTURE(id), {
                method: 'PATCH',
                headers: { 'Authorization': headers.Authorization },
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