"use strict";

const healthCheckUrl = "https://ecommerce-web.happymushroom-ad074909.japaneast.azurecontainerapps.io/health/ready";

const storeUrl = "https://ecommerce-web.happymushroom-ad074909.japaneast.azurecontainerapps.io";

const healthCheckInterval = 2000;

const loader = document.getElementById("loader");
const readyIcon = document.getElementById("ready-icon");

const statusTitle = document.getElementById("status-title");
const statusMessage = document.getElementById("status-message");
const statusText = document.getElementById("status-text");

const enterStoreButton = document.getElementById("enter-store-button");


function showReadyState() {
    loader.hidden = true;
    readyIcon.hidden = false;

    statusTitle.textContent = "商城已準備完成";

    statusMessage.textContent =
        "DIMIKO 商城已經準備完成，您現在可以進入商城";

    statusText.textContent = "服務已啟動";

    enterStoreButton.href = storeUrl;
    enterStoreButton.hidden = false;
}


async function checkHealth() {
    try {
        const response = await fetch(healthCheckUrl, {
            method: "GET",
            cache: "no-store"
        });

        console.log("Health Check Status:", response.status);

        if (response.ok) {
            const result = await response.text();

            console.log("ECommerce.Web:", result);
            console.log("商城已準備完成");

            showReadyState();

            return;
        }
    } catch (error) {
        console.log("商城尚未準備完成，稍後重新檢查...");
    }

    setTimeout(checkHealth, healthCheckInterval);
}

checkHealth();
