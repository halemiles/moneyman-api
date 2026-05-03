import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
const BASE_URL = process.env.MONEYMAN_API_URL ?? "http://localhost:5000";
// ── HTTP helpers ─────────────────────────────────────────────────────────────
async function apiGet(path) {
    const res = await fetch(`${BASE_URL}${path}`);
    if (!res.ok)
        throw new Error(`GET ${path} → ${res.status}: ${await res.text()}`);
    return res.json();
}
async function apiPost(path, body) {
    const res = await fetch(`${BASE_URL}${path}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });
    if (!res.ok)
        throw new Error(`POST ${path} → ${res.status}: ${await res.text()}`);
    return res.json();
}
async function apiPut(path, body) {
    const res = await fetch(`${BASE_URL}${path}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });
    if (!res.ok)
        throw new Error(`PUT ${path} → ${res.status}: ${await res.text()}`);
}
async function apiPatch(path) {
    const res = await fetch(`${BASE_URL}${path}`, { method: "PATCH" });
    if (!res.ok)
        throw new Error(`PATCH ${path} → ${res.status}: ${await res.text()}`);
}
async function apiDelete(path) {
    const res = await fetch(`${BASE_URL}${path}`, { method: "DELETE" });
    if (!res.ok)
        throw new Error(`DELETE ${path} → ${res.status}: ${await res.text()}`);
}
// ── Enum maps ─────────────────────────────────────────────────────────────────
const FREQUENCY = { Yearly: 0, Monthly: 1, Weekly: 2, Daily: 3, Anticipated: 4 };
const PAYMENT_TYPE = { MANUAL: 0, STANDINGORDER: 1, DIRECTDEBIT: 2, SUBSCRIPTION: 3 };
const CATEGORY_TYPE = {
    OTHER: 0, HOUSING: 1, TRANSPORTATION: 2, PERSONALCARE: 3, ENTERTAINMENT: 4,
    TAXES: 5, INSURANCE: 6, LOANS: 7, SAVINGS: 8, FOOD: 9, SERVICES: 10,
};
const PRIORITY_TYPE = { DESIRED: 0, DEPENDANT: 1, ESSENTIAL: 2 };
// ── Zod schemas (reused across tools) ────────────────────────────────────────
const FrequencyEnum = z.enum(["Yearly", "Monthly", "Weekly", "Daily", "Anticipated"]);
const PaymentTypeEnum = z.enum(["MANUAL", "STANDINGORDER", "DIRECTDEBIT", "SUBSCRIPTION"]);
const CategoryTypeEnum = z.enum(["OTHER", "HOUSING", "TRANSPORTATION", "PERSONALCARE", "ENTERTAINMENT", "TAXES", "INSURANCE", "LOANS", "SAVINGS", "FOOD", "SERVICES"]);
const PriorityTypeEnum = z.enum(["DESIRED", "DEPENDANT", "ESSENTIAL"]);
// ── Helpers ──────────────────────────────────────────────────────────────────
function fmt(date) {
    return new Date(date).toLocaleDateString("en-GB");
}
function gbp(n) {
    return `£${n.toFixed(2)}`;
}
function text(t) {
    return { content: [{ type: "text", text: t }] };
}
// ── MCP Server ────────────────────────────────────────────────────────────────
const server = new McpServer({ name: "moneyman", version: "1.0.0" });
// 1. Transactions due till payday
server.tool("get_transactions_till_payday", "Get all transactions due between now and the next payday, with totals and spend-per-week", { startingBalance: z.number().optional().describe("Optional current account balance in GBP") }, async ({ startingBalance }) => {
    const qs = startingBalance != null ? `?startingValue=${Math.round(startingBalance)}` : "";
    const dtp = await apiGet(`/dtp/current${qs}`);
    const lines = [
        `Period: ${fmt(dtp.startDate)} → ${fmt(dtp.endDate)}`,
        `Total due:      ${gbp(dtp.amountDue)}`,
        `Spend per week: ${gbp(dtp.spendPerWeek)}`,
        `Weeks remaining: ${dtp.weeksRemaining}`,
        ...(dtp.remaining != null ? [`Remaining after bills: ${gbp(dtp.remaining)}`] : []),
        "",
        `Upcoming transactions (${dtp.planDates.length}):`,
        ...dtp.planDates
            .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
            .map((p) => `  ${fmt(p.date).padEnd(12)} ${p.transactionName.padEnd(30)} ${gbp(p.amount)}`),
    ];
    return text(lines.join("\n"));
});
// 2. Days till payday
server.tool("days_till_payday", "Show how many days remain until the next payday and the payday date", {}, async () => {
    const dtp = await apiGet("/dtp/current");
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const payday = new Date(dtp.endDate);
    payday.setHours(0, 0, 0, 0);
    const diff = Math.round((payday.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    let msg;
    if (diff > 0)
        msg = `${diff} day${diff === 1 ? "" : "s"} until payday (${fmt(dtp.endDate)})`;
    else if (diff === 0)
        msg = `Payday is today! (${fmt(dtp.endDate)})`;
    else
        msg = `Payday was ${Math.abs(diff)} day${Math.abs(diff) === 1 ? "" : "s"} ago (${fmt(dtp.endDate)})`;
    return text(msg);
});
// 3. List transactions
server.tool("list_transactions", "List all transactions tracked in MoneyMan", { anticipated: z.boolean().optional().describe("If true, return only anticipated (one-off) transactions") }, async ({ anticipated }) => {
    const qs = anticipated != null ? `?anticipated=${anticipated}` : "";
    const txns = await apiGet(`/transaction${qs}`);
    if (!txns.length)
        return text("No transactions found.");
    const lines = txns.map((t) => `[${String(t.id).padStart(3)}] ${(t.name ?? "").padEnd(35)} ${gbp(t.amount ?? 0).padStart(9)}  active:${t.active}`);
    return text(`${txns.length} transaction(s):\n` + lines.join("\n"));
});
// 4. Create transaction
server.tool("create_transaction", "Create a new recurring or one-off transaction (bill, standing order, subscription, etc.)", {
    name: z.string().describe("Transaction name, e.g. 'Electricity Bill'"),
    amount: z.number().positive().describe("Amount in GBP, e.g. 100.00"),
    startDate: z.string().describe("Start date ISO 8601, e.g. '2026-05-10'"),
    frequency: FrequencyEnum.default("Monthly"),
    paymentType: PaymentTypeEnum.default("DIRECTDEBIT"),
    categoryType: CategoryTypeEnum.optional(),
    priorityType: PriorityTypeEnum.optional(),
    bankAccountId: z.number().int().optional().describe("Bank account ID to associate with this transaction"),
}, async ({ name, amount, startDate, frequency, paymentType, categoryType, priorityType, bankAccountId }) => {
    const body = {
        name,
        amount,
        startDate,
        active: true,
        frequency: FREQUENCY[frequency],
        paymentType: PAYMENT_TYPE[paymentType],
        ...(bankAccountId != null && { bankAccountId }),
        ...(categoryType && { categoryType: CATEGORY_TYPE[categoryType] }),
        ...(priorityType && { priorityType: PRIORITY_TYPE[priorityType] }),
    };
    const result = await apiPost("/transaction", body);
    return text(`Created "${name}" (ID ${result.id}) — ${gbp(amount)} ${frequency.toLowerCase()}, starts ${fmt(startDate)}`);
});
// 5. Update transaction (find by name or ID)
server.tool("update_transaction", "Update an existing transaction. Finds by name (fuzzy) if ID not provided.", {
    id: z.number().int().optional().describe("Transaction ID (use if known)"),
    name: z.string().optional().describe("Name to search for if ID unknown, e.g. 'electricity'"),
    amount: z.number().positive().optional().describe("New amount in GBP"),
    startDate: z.string().optional().describe("New start date ISO 8601"),
    active: z.boolean().optional(),
    frequency: FrequencyEnum.optional(),
    paymentType: PaymentTypeEnum.optional(),
    categoryType: CategoryTypeEnum.optional(),
    priorityType: PriorityTypeEnum.optional(),
}, async ({ id, name, amount, startDate, active, frequency, paymentType, categoryType, priorityType }) => {
    let existing;
    if (id != null) {
        existing = await apiGet(`/transaction/${id}`);
    }
    else if (name) {
        const all = await apiGet("/transaction");
        const match = all.find((t) => t.name?.toLowerCase().includes(name.toLowerCase()));
        if (!match)
            return text(`No transaction found matching "${name}". Use list_transactions to see all.`);
        existing = match;
    }
    else {
        return text("Provide either id or name to identify the transaction.");
    }
    const updated = {
        ...existing,
        ...(amount != null && { amount }),
        ...(startDate && { startDate }),
        ...(active != null && { active }),
        ...(frequency && { frequency: FREQUENCY[frequency] }),
        ...(paymentType && { paymentType: PAYMENT_TYPE[paymentType] }),
        ...(categoryType && { categoryType: CATEGORY_TYPE[categoryType] }),
        ...(priorityType && { priorityType: PRIORITY_TYPE[priorityType] }),
    };
    await apiPut("/transaction", updated);
    const changes = [];
    if (amount != null)
        changes.push(`amount → ${gbp(amount)}`);
    if (startDate)
        changes.push(`startDate → ${fmt(startDate)}`);
    if (active != null)
        changes.push(`active → ${active}`);
    if (frequency)
        changes.push(`frequency → ${frequency}`);
    if (paymentType)
        changes.push(`paymentType → ${paymentType}`);
    if (categoryType)
        changes.push(`categoryType → ${categoryType}`);
    if (priorityType)
        changes.push(`priorityType → ${priorityType}`);
    return text(`Updated "${existing.name}" (ID ${existing.id})${changes.length ? ": " + changes.join(", ") : ""}`);
});
// 6. Delete transaction
server.tool("delete_transaction", "Delete a transaction by ID", { id: z.number().int().describe("Transaction ID to delete") }, async ({ id }) => {
    await apiDelete(`/transaction/${id}`);
    return text(`Deleted transaction ID ${id}`);
});
// 7. Search plan dates
server.tool("search_plan_dates", "Search scheduled plan dates by transaction name", { transactionName: z.string().describe("Transaction name to search for") }, async ({ transactionName }) => {
    const planDates = await apiGet(`/plandate/search?transactionName=${encodeURIComponent(transactionName)}`);
    if (!planDates.length)
        return text(`No plan dates found for "${transactionName}"`);
    const lines = planDates
        .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
        .map((p) => `  ${fmt(p.date).padEnd(12)} ${gbp(p.amount)}`);
    return text(`Plan dates for "${transactionName}":\n` + lines.join("\n"));
});
// 8. Mark plan date paid
server.tool("mark_plan_date_paid", "Mark a specific plan date entry as paid by its ID", { id: z.number().int().describe("Plan date ID to mark as paid") }, async ({ id }) => {
    await apiPatch(`/plandate/${id}/paid`);
    return text(`Marked plan date ${id} as paid.`);
});
// 9. Regenerate plan dates
server.tool("generate_plan_dates", "Regenerate the DTP schedule (plan dates) for all transactions or a single one", { transactionId: z.number().int().optional().describe("Limit regeneration to a specific transaction ID") }, async ({ transactionId }) => {
    const qs = transactionId != null ? `?transactionId=${transactionId}` : "";
    const result = await apiPost(`/dtp/generate${qs}`);
    return text(`Generated ${result.recordCount} plan date(s). ${result.message}`);
});
// ── Start ─────────────────────────────────────────────────────────────────────
const transport = new StdioServerTransport();
server.connect(transport).then(() => {
    process.stderr.write(`Moneyman MCP server running — API: ${BASE_URL}\n`);
}).catch((err) => {
    process.stderr.write(`Moneyman MCP server failed to start: ${String(err)}\n`);
    process.exit(1);
});
