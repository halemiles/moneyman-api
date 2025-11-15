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
    // async sendPostRequest(endpoint: string, data: any): Promise<Response> {
    //     return await this.page.request.post(`${this.baseUrl}/${endpoint}`, { data });
    // }
    //
    // // Helper method to send a GET request
    // async sendGetRequest(endpoint: string): Promise<Response> {
    //     return await this.page.request.get(`${this.baseUrl}/${endpoint}`);
    // }
    //
    // // Helper method to send a DELETE request
    // async sendDeleteRequest(endpoint: string): Promise<Response> {
    //     return await this.page.request.delete(`${this.baseUrl}/${endpoint}`);
    // }
    //
    // // Method to generate plandates
    // async generatePlandates(dayOfMonth: number): Promise<Response> {
    //     await this.sendPostRequest('payday/generate', { DayOfMonth: dayOfMonth });
    //     return await this.sendGetRequest('dtp/generate');
    // }
    //
    // // Method to clear paydates
    // async clearPaydates(): Promise<Response> {
    //     return await this.sendDeleteRequest('payday/clear');
    // }
}