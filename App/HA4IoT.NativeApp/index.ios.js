/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 */
'use strict';

var React = require('react-native');
var {
  AppRegistry,
  ListView,
  ProgressViewIOS,
  ScrollView,
  SegmentedControlIOS,
  StyleSheet,
  SwitchIOS,
  Text,
  View
} = React;
var API = require('./API.js');

var Lamp = React.createClass({
  render: function() {
    return (
      <View style={{flexDirection: 'row'}}>
        <SwitchIOS value={this.props.state === "On"}/>
        <Text style={styles.lampText}>
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

var Socket = React.createClass({
  render: function() {
    return (
      <View style={{flexDirection: 'row'}}>
        <SwitchIOS value={this.props.state === 'On'}/>
        <Text style={styles.socketText}>
          {this.props.id}
        </Text>
      </View>
    )
  }
});

var MotionDetector = React.createClass({
  render: function() {
    return (
      <View style={{flexDirection: 'row'}}>
        <SwitchIOS value={this.props.state === 'On'} disabled={true} />
        <Text style={styles.motionDetectorText}>
          {this.props.id}
        </Text>
      </View>
    );
  }
});

var Button = React.createClass({
  render: function() {
    var buttonStyle = this.props.isEnabled
                    ? styles.buttonEnabled
                    : styles.buttonDisabled;
    var buttonText = this.props.isEnabled
                   ? 'On'
                   : 'Off';
    return (
      <View style={{flexDirection: 'row'}}>
        <View style={buttonStyle}>
          <Text>{buttonText}</Text>
        </View>
        <Text style={styles.buttonText}>{this.props.id}</Text>
      </View>
    );
  }
});

var RollerShutter = React.createClass({
  values: ['Up', 'Stopped', 'Down'],

  render: function() {
    var selectedIndex = this.values.indexOf(this.props.state);
    return (
      <View>
        <Text style={{marginBottom: 5}}>{this.props.id}</Text>
        <ProgressViewIOS progress={this.props.position / this.props.positionMax}
                         width={200}
                         height={10}
                         progressTintColor='#11EE11'
                         trackTintColor='#DDDDDD' />
        <SegmentedControlIOS values={this.values}
                             selectedIndex={selectedIndex}
                             width={200}
                             marginTop={5} />
      </View>
    );
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

      case 'HA4IoT.Actuators.Socket':
        return <Socket id={this.props.id} state={this.props.status.state} />;
        break;

      case 'HA4IoT.Actuators.MotionDetector':
        return <MotionDetector id={this.props.id}
                               isEnabled={this.props.status.isEnabled}
                               state={this.props.status.state} />;
        break;

      case 'HA4IoT.Actuators.Button':
        return <Button id={this.props.id}
                       isEnabled={this.props.status.isEnabled} />;
        break;

      case 'HA4IoT.Actuators.RollerShutter':
        return <RollerShutter id={this.props.id}
                              state={this.props.status.state}
                              position={this.props.status.position}
                              positionMax={this.props.status.positionMax} />;
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
                        .filter((a) => a.type !== 'HA4IoT.Actuators.Button')
                        .map((a) =>
                          <Actuator key={a.id}
                                    id={a.id}
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
      configuration: {},
      status: {}
    }
  },

  componentWillMount: function() {
    API.getConfiguration()
       .then((config) => this.setState({configuration: config}));
    API.getStatus()
       .then((status) => this.setState({status: status}));
  },

  render: function() {
    var roomComponents = Object.keys(this.state.configuration)
                               .map((room) =>
                                 <Room key={room}
                                       title={room}
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
    padding: 5,
    borderTopWidth: 1,
    borderColor: '#cccccc',
  },
  buttonText: {
    marginTop: 4,
    marginLeft: 8
  },
  buttonEnabled: {
    backgroundColor: '#11FF11',
    borderWidth: 2,
    borderColor: '#22EE22',
    justifyContent: 'center',
    alignItems: 'center',
    width: 50,
    height: 25
  },
  buttonDisabled: {
    backgroundColor: '#FF1111',
    borderWidth: 2,
    borderColor: '#EE2222',
    justifyContent: 'center',
    alignItems: 'center',
    width: 50,
    height: 25
  },
  lampText: {
    marginTop: 6,
    marginLeft: 6
  },
  socketText: {
    marginTop: 6,
    marginLeft: 6
  },
  motionDetectorText: {
    marginTop: 6,
    marginLeft: 6
  }
});

AppRegistry.registerComponent('HomeAutomationClient',
                              () => HomeAutomationClient);
