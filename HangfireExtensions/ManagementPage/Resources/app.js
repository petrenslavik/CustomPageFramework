﻿document.addEventListener("DOMContentLoaded", ready);
function ready() {
    const html = document.getElementById("app");
    const { createApp } = Vue
    createApp({
        data() {
            var data = {
                tableName: html.dataset.tableName,
                row: JSON.parse(JSON.parse(html.dataset.rows)[0]),
                saveUrl: html.dataset.saveUrl,
                settings: {
                    serverType: "",
                    serverUrl: "",
                    username: "",
                    password: ""
                },
                showSuccessNotification: false,
                showErrorNotification: false,
                showValidationError: false
            };
            if (data.row["SettingJson"]) {
                data.settings = JSON.parse(data.row["SettingJson"]);
            }
            return data;
        },
        methods: {
            remove(rowIndex) {
                this.rows.splice(rowIndex, 1);
            },
            add() {
                this.rows.push("");
            },
            validate() {
                if (!this.settings.serverUrl) {
                    this.showValidationError = true;
                } else {
                    this.showValidationError = false;
                }
            },
            save() {
                if (!this.settings.serverUrl) {
                    this.showValidationError = true;
                    return;
                }
                this.showValidationError = false;
                
                var newRow = Object.assign({}, this.row);
                newRow["SettingJson"] = JSON.stringify(this.settings);

                const params = {
                    "tableName": this.tableName,
                    "data": JSON.stringify(newRow)
                };

                const data = Object.keys(params)
                    .map((key) => `${key}=${encodeURIComponent(params[key])}`)
                    .join('&');

                var csrfHeader = $('meta[name="csrf-header"]').attr('content');
                var csrfToken = $('meta[name="csrf-token"]').attr('content');
                var headers = {
                    'content-type': 'application/x-www-form-urlencoded'
                };
                headers[csrfHeader] = csrfToken;

                axios.post(this.saveUrl, data, { headers }).then(() => {
                    this.showSuccessNotification = true;
                    setTimeout(() => {
                        this.showSuccessNotification = false;
                    }, 4000);
                }).catch(() => {
                    this.showErrorNotification = true;

                    setTimeout(() => {
                        this.showErrorNotification = false;
                    }, 4000);
                });
            }
        }
    }).mount(html)
}