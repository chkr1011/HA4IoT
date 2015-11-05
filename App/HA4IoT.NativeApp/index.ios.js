/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 */
'use strict';

var React = require('react-native');
var {
  AppRegistry,
  ListView,
  StyleSheet,
  ScrollView,
  SwitchIOS,
  Text,
  View
} = React;
var exampleConfig = require('./Configuration.js');
var exampleStatus = require('./Status.js');

var Room = React.createClass({
  render: function() {
    var actuators = this.props.config.actuators
                        .map((a) =>
                          <View style={styles.actuatorList}>
                            <SwitchIOS value={this.props.status[a.id].state === "On"}/>
                            <Text style={{marginTop: 6, marginLeft: 6}}>
                              {a.id}
                            </Text>
                          </View>)
      return (
        <View style={{marginBottom: 30}}>
          <Text style={styles.roomHeader}>{this.props.title}</Text>
          {actuators}
        </View>
      );
  }
});

var HomeAutomationClient = React.createClass({
  getInitialState: function() {
    return {
      configuration: exampleConfig,
      status: exampleStatus
    }
  },

  render: function() {
    var roomComponents = Object.keys(this.state.configuration)
                               .map((room) =>
                                 <Room title={room}
                                       config={this.state.configuration[room]}
                                       status={this.state.status} />
                               );
    return (
      <View style={styles.container}>
        <ScrollView>
          {roomComponents}
        </ScrollView>
      </View>
    );
  }
});

var styles = StyleSheet.create({
  container: {
    flex: 1,
    marginTop: 20
  },
  roomHeader: {
    fontSize: 30,
    marginBottom: 5
  },
  actuatorList: {
    flexDirection: 'row',
    alignItems: 'stretch',
    marginTop: 5
  }
});

AppRegistry.registerComponent('HomeAutomationClient',
                              () => HomeAutomationClient);
