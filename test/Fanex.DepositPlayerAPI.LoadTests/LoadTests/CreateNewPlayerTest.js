import { randomString } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { sleep } from "k6";
import http from "k6/http";
import { Counter } from "k6/metrics";
import { Env, BaseUrlByEnv, ApiKeyByEnv } from "./Configuration.js"

export const CounterErrors = new Counter('errors');

export const options = {
    // need to config stages before starting
    //stages: [
    //    { duration: "1s", target: 2 },
    //    { duration: "8s", target: 2 },
    //    { duration: "1s", target: 0 },
    //],
    thresholds: {
        errors: ["count<1"],
    },
    noConnectionReuse: true,
};

export default function () {
    sendRequest(Env.LOCAL);
    sleep(1);
}

const DefaultDataByEnv = {};

DefaultDataByEnv[Env.LOCAL] = {
    username: 'kai.test.',
    userId: 40,
    password: "Robin0123",
    oddsType: "ABC",
    agentId: 836210,
    affiliateId: 927841,
    country: "VietNam",
    subsidiaryID: 100,
};

DefaultDataByEnv[Env.SIT] = {
    username: "Test22081_KaiTest_",
    userId: 2147,
    password: "BC48AACE833997B8618F1D18A40D8A",
    oddsType: "a",
    agentId: 10063439,
};

const sendRequest = (env) => {
    const url = BaseUrlByEnv[env] + "/api/deposit/players";
    // const url = "https://localhost:7086/api/players";

    const randomText = randomString(3);
    const timestamp = Date.now();
    const username = DefaultDataByEnv[env].username + `${randomText}${timestamp}`;
    const payload = JSON.stringify({
        ...DefaultDataByEnv[env],
        username: username,
        isDebug: true
    });

    const params = {
        headers: {
            "Content-Type": "application/json",
            "ApiKey": ApiKeyByEnv[env],
        },
    };

    const res = http.post(url, payload, params);
    const data = res.json();
    const isSuccess = res.status === 200 && data.errorCode === 0 && data.result !== null;

    CounterErrors.add(!isSuccess);
}

// macOs: K6_WEB_DASHBOARD=true K6_WEB_DASHBOARD_EXPORT="./Reports/CreateNewPlayerLoadTests.html" k6 run CreateNewPlayerLoadTests.js
// windows: $env:K6_WEB_DASHBOARD="true" ; $env:K6_WEB_DASHBOARD_EXPORT="./Reports/CreateNewPlayerLoadTests.html" ; k6 run CreateNewPlayerLoadTests.js
