using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class MovePlayerUp : EnvBaseAgent
    {
        private IDistanceRewarder upDistanceRewarder;
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 playerPos = controller.player.localPosition;
            sensor.AddObservation(playerPos / controller.env.Width * 2f);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            upDistanceRewarder = new OnlyImprovingDistanceRewarder(controller.env.DistancePlayerX1FromRight);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            if (PostCondition != null && PostCondition.Func())
            {
                Debug.Log("Up! PC: " + PostCondition.Name);
                AddReward(-0.0f * controller.rb.velocity.magnitude / controller.maxSpeed);
            }
            AddReward(upDistanceRewarder.Reward() * 1f);
        }
    }
}
