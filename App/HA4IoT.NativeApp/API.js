'use strict';

var exampleConfig = require('./Configuration.js');
var exampleStatus = require('./Status.js');

var API = {
  getConfiguration: function() {
    return new Promise(function(resolve, reject) {
      resolve(exampleConfig);
    });
  },

  getStatus: function() {
    return new Promise(function(resolve, reject) {
      resolve(exampleStatus);
    });
  }
};

module.exports = API;
