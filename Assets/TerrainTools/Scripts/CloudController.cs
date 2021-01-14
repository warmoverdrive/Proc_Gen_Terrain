using UnityEngine;

public class CloudController : MonoBehaviour
{
	ParticleSystem cloudSystem;
	public Color color;
	public Color lining;
	public bool painted = false;
	public int numberOfParticles;
	public float minSpeed;
	public float maxSpeed;
	public float distance;
	Vector3 startPosition;
	float speed;

	void Start()
	{
		cloudSystem = GetComponent<ParticleSystem>();
		Spawn();
	}

	void Spawn()
	{
		// extend the range of the scale on either side of the manager center
		float xPos = Random.Range(-0.5f, 0.5f);
		float yPos = Random.Range(-0.5f, 0.5f);
		float zPos = Random.Range(-0.5f, 0.5f);
		transform.localPosition = new Vector3(xPos, yPos, zPos);
		speed = Random.Range(minSpeed, maxSpeed);
		startPosition = transform.position;
	}

	void Paint()
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cloudSystem.particleCount];
		cloudSystem.GetParticles(particles);
		// check if particles have been instantiated 
		if (particles.Length > 0)
		{
			for (int i = 0; i < particles.Length; i++)
				particles[i].startColor = Color.Lerp(lining, color,
					particles[i].position.y / cloudSystem.shape.scale.y); ;
			painted = true;
			cloudSystem.SetParticles(particles, particles.Length);
		}
	}

	void Update()
	{
		if (!painted)
			Paint();

		transform.Translate(0, 0, speed);

		if (Vector3.Distance(startPosition, transform.position) > distance)
			Spawn();
	}
}
