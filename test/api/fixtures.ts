import { test as base, expect } from "@playwright/test";

/**
 * A stable identifier for the whole `npx playwright test` invocation. Every request
 * in the run shares this value (Sentry tag `test.run_id`), so you can isolate the
 * server-side events produced by a single test run. Override via TEST_RUN_ID in CI.
 */
const RUN_ID = process.env.TEST_RUN_ID ?? `local-${new Date().toISOString()}`;

/**
 * Extends the base test so that every request issued through the `request`/`page`
 * fixtures carries the current test name and run id. The API tags its Sentry scope
 * from these headers (see SentryTestContextMiddleware), letting a failing test be
 * traced to its exact server-side error.
 */
export const test = base.extend({
  extraHTTPHeaders: async ({ extraHTTPHeaders }, use, testInfo) => {
    await use({
      ...extraHTTPHeaders,
      "X-Test-Name": testInfo.title,
      "X-Test-Run-Id": RUN_ID,
    });
  },
});

export { expect };
