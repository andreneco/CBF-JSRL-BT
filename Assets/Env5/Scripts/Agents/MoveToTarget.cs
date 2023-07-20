using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MoveToTarget : EnvBaseAgent
    {
        private IDistanceRewarder playerTrigger1DistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            Vector3 trigger1Pos = controller.env.trigger1.localPosition;
            Vector3 distanceObs = (trigger1Pos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            playerTrigger1DistanceRewarder = new OnlyImprovingDistanceRewarder(controller.DistanceToTrigger1);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Trigger1 reached! PC: " + PostCondition.Name);

                float velocityPunishment = -0.1f * controller.rb.velocity.magnitude / controller.maxSpeed;
                AddReward(velocityPunishment);
            }
            AddReward(playerTrigger1DistanceRewarder.Reward() * 1f);
        }
    }
}
