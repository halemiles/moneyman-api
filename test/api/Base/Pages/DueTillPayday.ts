import {APIRequestContext, Page} from '@playwright/test';

export class DuetillPaydayPage {
    private page: Page;
    private request: APIRequestContext

    constructor(page: Page, request:APIRequestContext) {
        this.page = page;
        this.request = request;
    }

    // Base URL for the application
    private get baseUrl() {
        return "http://localhost:5000";
    }

    // Helper method to send a POST request
    public async generatePlandates(): Promise<Response>{
        return await this.request.get(`${this.baseUrl}/dtp/generate`);
    }
}