namespace HA4IoT.Contracts.PersonalAgent.AmazonEcho
{
    public class SkillServiceRequest
    {
        public string Version { get; set; }

        public SkillServiceRequestSession Session { get; set; } = new SkillServiceRequestSession();

        public SkillServiceRequestRequest Request { get; set; } = new SkillServiceRequestRequest();
    }
}

/*

{
  "session": {
    "sessionId": "SessionId.c8699539-3df7-4fdf-9fcd-2a57a9c3a59d",
    "application": {
      "applicationId": "amzn1.ask.skill.55a7c2ab-b3e4-412d-8f2f-7204e394864d"
    },
    "attributes": {},
    "user": {
      "userId": "amzn1.ask.account.AFUW7KUWQMSERIFZUZKIYKUIY7IQ4YR76CWXY4NTASC7PH2POXEDFZ2FO53DR7IS5VSB5HEQS747KL74RCWQQJ7BKILGXWNA6PPSMNT34COUNM7NXVJ33GSI5IMMJIOWN4NP4SBNE7EO2HYPQQLCB55FFZCNVRUFD6ZXPK2LRBSEFVYCNOML6EE4EN7D4AQNLII6UCS353CXKPA"
    },
    "new": true
  },
  "request": {
    "type": "IntentRequest",
    "requestId": "EdwRequestId.6fee4642-356f-4f2d-9b47-6e7e50481335",
    "locale": "de-DE",
    "timestamp": "2016-12-25T23:24:14Z",
    "intent": {
      "name": "TurnOn",
      "slots": {
        "Area": {
          "name": "Area",
          "value": "Wohnzimmer"
        },
        "Component": {
          "name": "Component",
          "value": "Licht"
        }
      }
    }
  },
  "version": "1.0"
}

 */
