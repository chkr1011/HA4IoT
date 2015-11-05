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

var Lamp = React.createClass({
  render: function() {
    return (
      <View style={{flexDirection: 'row'}}>
        <SwitchIOS value={this.props.state === "On"}/>
        <Text style={{marginTop: 6, marginLeft: 6}}>
          {this.props.id}
        </Text>
      </View>
    )
  }
});

var TemperatureSensor = React.createClass({
  render: function() {
    return (
      <Text>{Math.round(this.props.temperature)}˚ C</Text>
    )
  }
});

var HumiditySensor = React.createClass({
  render: function() {
    return (
      <Text>{Math.round(this.props.humidity)}%</Text>
    )
  }
});

var Actuator = React.createClass({
  getComponentForType: function() {
    switch (this.props.type) {
      case 'HA4IoT.Actuators.Lamp':
        return <Lamp id={this.props.id} state={this.props.status.state} />;
        break;

      case 'HA4IoT.Actuators.TemperatureSensor':
        return <TemperatureSensor id={this.props.id}
                                  temperature={this.props.status.value} />;
        break;

      case 'HA4IoT.Actuators.HumiditySensor':
        return <HumiditySensor id={this.props.id}
                               humidity={this.props.status.value} />;
        break;

      default:
        return <Text>Implement me! ({this.props.type})</Text>;
        break;
    }
  },

  render: function() {
    return (
      <View style={styles.actuatorRow}>
        {this.getComponentForType()}
      </View>
    )
  }
});

var Room = React.createClass({
  render: function() {
    var actuators = this.props.config.actuators
                        .map((a) =>
                          <Actuator id={a.id}
                                    type={a.type}
                                    status={this.props.status[a.id]} />)
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
  actuatorRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: 5,
    marginRight: 5,
    borderTopWidth: 1,
    borderColor: '#cccccc',
    height: 50
  }
});

AppRegistry.registerComponent('HomeAutomationClient',
                              () => HomeAutomationClient);
