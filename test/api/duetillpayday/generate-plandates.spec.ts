import { test, expect } from '@playwright/test';
import {dtp, dtpResponse} from '../models/dtp';

const BASE_URL = "http://localhost:5000"; // Adjust if needed
test('Generate plandates', async ({ request }) => {
    const paydayGenerationResponse = await request.post(`${BASE_URL}/payday/generate`,{
        data:{DayOfMonth: 25}
    })
    const plandateResponse = await request.get(`${BASE_URL}/dtp/generate`);

    const  body = await plandateResponse.json();

    expect(body.message).toBe("Success");
    expect(body.recordCount).toBeGreaterThan(0);
});


test('Generate plandates without paydates', async ({ request }) => {
    const plandateResponse = await request.delete(`${BASE_URL}/payday/clear`);
    console.log(await plandateResponse);
    const  body = await plandateResponse;

    expect(body.ok()).toBeTruthy();

    const dtpGenerationResponse = await request.get(`${BASE_URL}/dtp/generate`);

    const  dtpGenerationJson = await dtpGenerationResponse.json();
    expect(dtpGenerationJson.message).toBe("Missing Paydays");
    expect(dtpGenerationJson.recordCount).toBe(0);
});
