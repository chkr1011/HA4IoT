'use strict';

var exampleConfig = require('./Configuration.js');
var exampleStatus = require('./Status.js');

var API = {
  getConfiguration: function() {
    return new Promise(function(resolve, reject) {
      resolve(exampleConfig);
    });
  },

  pollConfiguration: function(callback) {
    this.getConfiguration().then((result) => callback(result));
    setTimeout(() => this.pollConfiguration(callback), 2000);
  },

  getStatus: function() {
    return new Promise(function(resolve, reject) {
      resolve(exampleStatus);
    });
  },

  pollStatus: function(callback) {
    this.getStatus().then((result) => callback(result));
    setTimeout(() => this.pollStatus(callback), 2000);
  }
};

module.exports = API;
