'use strict';

var statusUrl = 'http://192.168.1.15/api/status';
var configUrl = 'http://192.168.1.15/api/configuration';

var API = {
  getConfiguration: function() {
    return fetch(configUrl).then((response) => response.json());
  },

  getStatus: function() {
    return fetch(statusUrl).then((response) => response.json());
  },

  pollStatus: function(callback) {
    this.getStatus().then((result) => callback(result));
    setTimeout(() => this.pollStatus(callback), 250);
  }
};

module.exports = API;
