using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CarController : MonoBehaviour
{

    [Header("Network Information")]
    // Neural network for car
    private bool initialized = false;
    public bool alive = true;
    private NeuralNetwork net;
    public float checkPointReward = 15f;

    [Header("State")]
    public float totalDistanceTravelled;
    public float timeSinceStarted;
    Vector3 lastPosition, startPosition;

    [Header("Fitness")]
    public float overallFitness = 0;
    public float rewardUponDeath = -5;
    public float distanceMultiplier = 1.5f;
    public float collisionMultiplier = -1f;
    public float sensorDistanceMultiplier = 1.5f;
    public static ArrayList colliders = new ArrayList();


    [Header("Sensor Info")]

    public float sensorRange;
    public float sensorAvg = 0;
    public float moveSpeed;
    public float turnSpeed;
    public float moveLerpSteps;
    public float turnLerpSteps;
    public LayerMask mask;


    public Transform tip;
    Rigidbody rb;
    [Range(-1f, 1f)]
    public float x, y;


    //public Transform Car;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;
        y = Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        timeSinceStarted += Time.deltaTime;
        if (initialized && alive)
        {
            float[] inputs = Sensors();
            float[] outputs = net.FeedForward(inputs);
            Control(outputs[0], outputs[1]);

            //NN Fitness Information
            CalculateFitness();
        }
    }

    void Control(float gas, float steer)
    {
        //Movement
        //Method 1(Not so great, kinda failed)
        //rb.velocity = new Vector3(gas * moveSpeed * Time.deltaTime, 0, 0);
        //Method 2
        //transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x * gas * moveSpeed, transform.position.y, transform.position.z), moveLerpSteps * Time.deltaTime);
        //Method 3
        transform.Translate(Vector3.forward * gas *moveSpeed* Time.deltaTime);

        //Rotation
        //Method 1(Failed miserably)
        /*
        Quaternion angle = Quaternion.Euler(new Vector3(0, steer * turnSpeed * Time.deltaTime, 0));
        rb.MoveRotation(rb.rotation * angle);
        */

        //Method 2 (Field even miserably)
        //transform.eulerAngles = new Vector3(0, steer * turnSpeed * Time.deltaTime, 0);
        //Method 3
        transform.Rotate(0, steer *turnSpeed* Time.deltaTime, 0);


    }

    //Taking sensor Input, used raycast; 
    float[] Sensors()
    {

        float[] sensors = new float[5];
        RaycastHit hit;

        //Raycast directions; 
        Vector3 forward = tip.forward;
        Vector3 right = tip.right;
        Vector3 rightTilt = tip.forward + tip.right;
        Vector3 left = -tip.right;
        Vector3 leftTilt = tip.forward - tip.right;

        //Forward Ray;
        Ray ray = new Ray(tip.position, forward);


        if (Physics.Raycast(ray, out hit, sensorRange, mask))
        {
            sensors[0] = hit.distance;

            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else sensors[0] = -1;
        sensorAvg += sensors[0];


        //Right Ray;
        ray.direction = right;

        if (Physics.Raycast(ray, out hit, sensorRange, mask))
        {
            sensors[1] = hit.distance;


            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else sensors[1] = -1;
        sensorAvg += sensors[1];

        //Left Ray;
        ray.direction = left;

        if (Physics.Raycast(ray, out hit, sensorRange, mask))
        {
            sensors[2] = hit.distance;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else sensors[2] = -1;
        sensorAvg += sensors[2];

        //rightTilt Ray;
        ray.direction = rightTilt;

        if (Physics.Raycast(ray, out hit, sensorRange, mask))
        {
            sensors[3] = hit.distance;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else sensors[3] = -1;
        sensorAvg += sensors[3];


        //leftTilt Ray;
        ray.direction = leftTilt;

        if (Physics.Raycast(ray, out hit, sensorRange, mask))
        {
            sensors[4] = hit.distance;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else sensors[4] = -1;
        sensorAvg += sensors[4];
        sensorAvg /= sensors.Length;

        return sensors;
    }



    //NN stuff; 
    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Death();
        }

        if (other.gameObject.tag == "Check Points")
        {
            lastPosition = transform.position;
            //other.gameObject.SetActive(false);
            //sumFitness += checkPointReward;
            //checkPoints++;
            //net.AddFitness(checkPoints * sumFitness + checkPoints);
            colliders.Add(other);
        }
    }

    void Death()
    {
        rb.velocity = Vector3.zero;
        net.AddFitness(rewardUponDeath);
        alive = false;
    }

    void Reset()
    {
        alive = false;

        /*
         timeSinceStarted = 0f;
         totalDistanceTravelled = 0f;
         overallFitness = 0f;
         lastPosition = startPosition;
         */
        //transform.position = startPosition;
        // network.Initialize(layers, neurons);
    }

    void CalculateFitness()
    {
        float checkPoints = colliders.Count;
        float distance1 = 0;
        float distance2 = 0;

        if (colliders == null)
        {
            distance1 = Vector3.Distance(startPosition, transform.position);
        }
        else distance2 = Vector3.Distance(lastPosition, transform.position);

        net.AddFitness(sensorAvg * sensorDistanceMultiplier + checkPoints * checkPointReward + (distance1 + distance2) * distanceMultiplier);




    }



}
