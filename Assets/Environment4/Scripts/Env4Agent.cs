using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Env4Agent : Agent, IStateController
{
    public float speed = 1f;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform batteryTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public float batteryConsumption = 0.00f;
    private float battery = 1f;
    public float fieldWidth = 10f;
    public EnemyBehavior4 enemy;
    private CBFApplicator enemyCbfApplicator;
    private CBFApplicator wall1CBFApplicator;


    public override void OnEpisodeBegin()
    {
        // Reset the positions
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        targetTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        enemyTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        batteryTransform.gameObject.SetActive(true);

        battery = Random.Range(0.5f, 1f);

        enemyCbfApplicator = new CBFApplicator(new MovingBallCBF3D(1f), new CombinedState(this, enemy));
        wall1CBFApplicator = new CBFApplicator(new WallCBF3D(new Vector3(9f, 0f, 0f), new Vector3(1f, 0f, 0f)), this);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var factor = 1f;
        sensor.AddObservation(transform.localPosition.x / factor);
        sensor.AddObservation(transform.localPosition.z / factor);
        sensor.AddObservation(targetTransform.localPosition.x / factor);
        sensor.AddObservation(targetTransform.localPosition.z / factor);
        sensor.AddObservation(enemyTransform.localPosition.x / factor);
        sensor.AddObservation(enemyTransform.localPosition.z / factor);
        sensor.AddObservation(batteryTransform.localPosition.x / factor);
        sensor.AddObservation(batteryTransform.localPosition.z / factor);
        sensor.AddObservation(battery);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var allMasked = true;
        for (int i = 0; i < 9; i++)
        {
            var actions = new ActionBuffers(new float[] { }, new int[] { i });
            var okay = enemyCbfApplicator.actionOkayDiscrete(actions);
            if (!okay)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            else
            {
                actionMask.SetActionEnabled(0, i, true);
                allMasked = false;
            }
        }
        if (allMasked)
        {
            Debug.Log("All actions masked!");
            for (int i = 0; i < 9; i++)
            {
                actionMask.SetActionEnabled(0, i, true);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 movement = _controlledDynamics(actions);

        // Apply the movement
        transform.localPosition += movement * Time.deltaTime;

        AddReward(-0.5f / MaxStep);
        // battery -= batteryConsumption * movement.magnitude / MaxStep;
        if (battery <= 0)
        {
            AddReward(-1f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery empty!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    private Vector3 _controlledDynamics(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        var action = discreteActions[0];

        var i = action % 3;
        var j = action / 3;
        var movement = new Vector3(i - 1, 0f, j - 1) * speed;
        return movement;

        // var moveXAction = discreteActions[0];
        // var moveZAction = discreteActions[1];

        // float moveX = moveXAction - 1;
        // float moveZ = moveZAction - 1;

        // var movement = new Vector3(moveX, 0f, moveZ) * speed;
        // return movement;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreateActionsOut = actionsOut.DiscreteActions;

        var i = (int)Input.GetAxisRaw("Horizontal") + 1;
        var j = (int)Input.GetAxisRaw("Vertical") + 1;
        discreateActionsOut[0] = i + 3 * j;
        Debug.Log(discreateActionsOut[0]);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
            Debug.Log("Target reached!");
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Target reached!");
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Wall hit!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1.0f);
            Debug.Log("Reward: " + GetCumulativeReward() + " | Battery: " + battery + " | Enemy hit!");
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Battery"))
        {
            battery = 1f;
            AddReward(0.1f);
            batteryTransform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            // batteryTransform.gameObject.SetActive(false);
            Debug.Log("Battery collected!");
        }
    }

    public float[] currentState()
    {
        return Utility.vec3ToArr(transform.localPosition);
    }

    public float[] ControlledDynamics(ActionBuffers action)
    {
        return Utility.vec3ToArr(_controlledDynamics(action));
    }
}
