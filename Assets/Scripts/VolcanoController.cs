using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoController : MonoBehaviour
{

    [SerializeField]
    private int anger = 0;

    [SerializeField]
    private int maxAnger = 100;

    public GameObject lavablob;
    private float EjectionForce = 20.0f;
    private float TorqueForce = 10.0f;
    private float minLavaSize = 0.05f;
    private float maxLavaSize = 0.4f;
    private float ejectionsPerSecond = 0.5f;
    private float minEjectionsPerSecond = 0.5f;
    private float maxEjectionsPerSecond = 50f;
    private float AppeaseTime;
    [SerializeField]
    private float AppeaseCoolDownTime = 0.75f;
    private float angerIncrementSecs = 1.6f;

    [SerializeField]
    private Light lightFromVolcano;
    private float flickerLastTime;
    private float flickerRate = 0.3f; // secs
    private float oldLightRange=30f, newLightRange=30f;
    private float oldLightColor=0f, newLightColor=0f;

    public HealthBarController healthBar;

    int sacrificedCount = 0;
    int _sacrificedRequired = 5;

    [SerializeField]
    private AudioClip audioGreeting, audioAppeased;

    // Start is called before the first frame update
    void Start()
    {
        setAppeaseCoolDown(AppeaseCoolDownTime);
        setMaxAnger(maxAnger);
        flickerLastTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Animate lava (depends on anger level)
        if ( Random.Range(0f,1f) < (ejectionsPerSecond*Time.deltaTime) )
        {
            GameObject go = Instantiate(lavablob, transform.position+4.5f*Vector3.up, Quaternion.identity, transform);
            Vector3 scale = Random.Range(minLavaSize, maxLavaSize)*go.transform.localScale;
            go.transform.localScale = scale;
            Vector3 direction = new Vector3(Random.Range(-0.1f,0.1f), 1, Random.Range(-0.25f,0.1f));
            go.GetComponent<Rigidbody>().AddForce(EjectionForce* direction, ForceMode.Impulse);
            go.GetComponent<Rigidbody>().AddTorque(TorqueForce*Random.insideUnitSphere);
        }

        // Make angrier over time
        if ((Time.time - AppeaseTime) > 0)
        {
            if (Random.Range(0f, 1f) < (Time.deltaTime / angerIncrementSecs))
            {
                makeAngrier(5);
            }
        }

        // Flicker volcano light
        if ((Time.time-flickerLastTime) > flickerRate)
        {
            flickerLastTime = Time.time;
            oldLightRange = newLightRange;
            newLightRange = Random.Range(10f, 40f);
            oldLightColor = newLightColor;
            newLightColor = Random.Range(0f, 0.5f);
        }
        float t1 = (Time.time - flickerLastTime) / flickerRate;
        lightFromVolcano.range = Mathf.Lerp(oldLightRange, newLightRange, t1);
        float tmp = Mathf.Lerp(oldLightColor, newLightColor, t1);
        lightFromVolcano.color = new Color(1f, tmp, tmp);
    }

    private void setAngerLevel(int amount)
    {
        anger = Mathf.Clamp(amount, 0, maxAnger);
        ejectionsPerSecond = Mathf.Max(minEjectionsPerSecond, ((float)anger / (float)maxAnger) * maxEjectionsPerSecond);
        healthBar.SetHealth(anger);
    }

    public void makeAngrier(int amount)
    {
        setAngerLevel(anger + amount);
    }

    public void appease(int amount)
    {
        makeAngrier(-amount);
        AppeaseTime = Time.time + AppeaseCoolDownTime;
    }

    public void resetAnger()
    {
        setAngerLevel(0);
    }

    public float getAngerProportion()
    {
        return (float)anger / (float)maxAnger;
    }

    public bool hasErupted()
    {
        return anger == maxAnger;
    }

    public bool isAppeased()
    {
        return (sacrificedCount >= _sacrificedRequired);
    }

    public void giveSacrifice()
    {
        sacrificedCount += 1;
    }

    public int getSacrificed()
    {
        return sacrificedCount;
    }

    public int sacrificedRequired()
    {
        return _sacrificedRequired;
    }

    public void setSacrificesRequired(int sacrificedRequired)
    {
        _sacrificedRequired = sacrificedRequired;
    }

    public void setAngerToMax()
    {
        anger = maxAnger;
    }

    public void setMaxAnger(int newMaxAnger)
    {
        maxAnger = newMaxAnger;
        healthBar.SetMaxHealth(maxAnger);
        healthBar.SetHealth(anger);
    }

    public void setAppeaseCoolDown(float secs)
    {
        AppeaseCoolDownTime = secs;
        AppeaseTime = Time.time + AppeaseCoolDownTime;
    }

    public AudioClip getAudioGreeting()
    {
        return audioGreeting;
    }

    public AudioClip getAudioAppeased()
    {
        return audioAppeased;
    }

}
