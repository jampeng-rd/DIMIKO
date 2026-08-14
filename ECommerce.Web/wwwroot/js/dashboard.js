document.addEventListener("DOMContentLoaded", function () {

    const data = window.dashboardData;

    if (!data) {
        return;
    }


    // 每日營收
    const revenueCanvas = document.getElementById("dailyRevenueChart");

    if (revenueCanvas) {

        new Chart(revenueCanvas, {
            type: "line",

            data: {
                labels: data.dailyRevenue.map(item => `${item.Day} 日`),

                datasets: [{
                    label: "營收",
                    data: data.dailyRevenue.map(item => item.Revenue),
                    borderWidth: 2,
                    tension: 0.3
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }


    // 每日訂單
    const ordersCanvas = document.getElementById("dailyOrdersChart");

    if (ordersCanvas) {

        new Chart(ordersCanvas, {
            type: "bar",

            data: {
                labels: data.dailyOrders.map(item => `${item.Day} 日`),

                datasets: [{
                    label: "訂單數",
                    data: data.dailyOrders.map(item => item.Count),
                    borderWidth: 1
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }


    // 訂單狀態
    const statusCanvas = document.getElementById("orderStatusChart");

    if (statusCanvas) {

        new Chart(statusCanvas, {
            type: "doughnut",

            data: {
                labels: data.orderStatusBreakdown.map(item => item.Status),

                datasets: [{
                    data: data.orderStatusBreakdown.map(item => item.Count)
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        position: "bottom"
                    }
                }
            }
        });
    }


    // 商品分類 (用 Bar Chart)
    const categoryCanvas = document.getElementById("categoryProductChart");

    if (categoryCanvas) {

        new Chart(categoryCanvas, {
            type: "bar",

            data: {
                labels: data.productsPerCategory.map(item => item.Category),

                datasets: [{
                    label: "商品數量",

                    data: data.productsPerCategory.map(item => item.Count),

                    borderWidth: 1
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }


    // 商品庫存
    const stockCanvas = document.getElementById("productStockChart");
    const stockScroll = document.querySelector(".dashboard-stock-scroll");
    const stockChartContainer = document.getElementById("productStockChartContainer");

    if (stockCanvas &&
        stockScroll &&
        stockChartContainer &&
        data.productStocks
    ) {

        const stockItems = data.productStocks;

        // 每個商品保留固定寬度
        const widthPerProduct = 120;

        // 目前庫存卡片真正可看到的寬度
        const visibleWidth = stockScroll.getBoundingClientRect().width;

        // 所有商品需要的完整圖表寬度
        const requiredWidth = stockItems.length * widthPerProduct;

        // 商品少時填滿卡片；商品多時只放大內部圖表
        const chartWidth = Math.max(visibleWidth, requiredWidth);

        stockChartContainer.style.width = `${chartWidth}px`;

        new Chart(stockCanvas, {
            type: "bar",
            data: {
                labels:
                    stockItems.map(item => item.Title),

                datasets: [{
                    label: "庫存量",

                    data:
                        stockItems.map(item => item.StockQuantity),

                    backgroundColor:
                        stockItems.map(item => item.StockQuantity <= 5 ? "#dc3545" : "#36a2eb"),

                    borderColor:
                        stockItems.map(item => item.StockQuantity <= 5 ? "#dc3545" : "#36a2eb"),

                    borderWidth: 1
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    },

                    tooltip: {
                        callbacks: {
                            title: function (items) {
                                if (!items.length) {
                                    return "";
                                }

                                return stockItems[
                                    items[0].dataIndex
                                ].Title;
                            },

                            afterTitle: function (items) {
                                if (!items.length) {
                                    return "";
                                }

                                const product =
                                    stockItems[
                                    items[0].dataIndex
                                    ];

                                return `SKU：${product.SKU}`;
                            },

                            label: function (context) {
                                return `庫存：${context.raw} 件`;
                            }
                        }
                    }
                },

                scales: {

                    x: {

                        ticks: {
                            autoSkip: false,
                            maxRotation: 0,   // 名稱最大旋轉 角度
                            minRotation: 0    // 名稱最小旋轉 角度
                        }
                    },

                    y: {

                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }


});
