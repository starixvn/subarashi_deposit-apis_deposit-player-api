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
    sabaId: 2025,
    custId: 195849188,
    custType: 0,
};

DefaultDataByEnv[Env.SIT] = {
    sabaId: 2025,
    custId: 68921608,
    custType: 0,
};

const sendRequest = (env) => {
    const { sabaId, ...bodyData } = DefaultDataByEnv[env];
    const url = BaseUrlByEnv[env] + `/api/deposit/sabaids/${sabaId}/mapping`;
    // const url = `https://localhost:7086/api/players/${custId}`;

    const payload = JSON.stringify({
        ...bodyData,
        isDebug: true
    });

    const params = {
        headers: {
            "Content-Type": "application/json",
            "ApiKey": ApiKeyByEnv[env],
        },
    };

    const res = http.put(url, payload, params);
    const data = res.json();
    const isSuccess = res.status === 200 && data.errorCode === 0;

    CounterErrors.add(!isSuccess)
}

// macOs: K6_WEB_DASHBOARD=true K6_WEB_DASHBOARD_EXPORT="./Reports/MapSabaIdTest.html" k6 run MapSabaIdTest.js
// windows: $env:K6_WEB_DASHBOARD="true" ; $env:K6_WEB_DASHBOARD_EXPORT="./Reports/MapSabaIdTest.html" ; k6 run MapSabaIdTest.js
