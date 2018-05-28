using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using HardcoreShit;

public class Logger_GunStats : MonoBehaviour {

	/*public NestedGameObject[] body;
	public NestedGameObject[] barrel;
	public NestedGameObject[] magazine;
	public NestedGameObject[] scope;
	public NestedGameObject[] stock;

	[System.Serializable]
	public class NestedGameObject {
		public bool[] GunParts;
	}*/


	//public GunController c;
	//public GunBuilder b;

	public int maxValue = 100;
	public int resolution = 100;
	public Vector2 valueRange = new Vector2(50,150);

	public GameObject sliderPrefab;
	public GameObject sliderParent;

	GameObject[] sliders;

	[Range(-3f,3f)]
	public float test;
	public int test2;

	// Use this for initialization
	void Start () {
		//Invoke ("delayedStart", 1f);
		SetUpSliders();

		InvokeRepeating ("Test1", 0.1f, 1f);
        //InvokeRepeating("Test2", 0.1f, 1f);
        //Test();
    }

	void Test1(){

		int zero = 0;
		int one = 0;
		int two = 0;
		int three = 0;
		int four = 0;
		int wtf = 0;
        int total = 0;
        int calc = 0;

		for (int i = 0; i <= 10000; i++) {
			/*float allChances = Mathf.Pow (2, 4) - 1;
			allChances = Mathf.Pow (2, 4);
			allChances = Mathf.Pow (2, allChances);
			double random = (double)UnityEngine.Random.Range (0f, (float)allChances);
			//int ourSize = Mathf.ClosestPowerOfTwo (random);*/



			//print ("-------------");
			//print (allChances);
			//print (random);
			//print (Mathf.Sqrt (random));
			//random = cust;
			/*random = Mathf.Sqrt ((float)random);
			random = Mathf.Sqrt ((float)random);
			random /= 1;*/

			/*random = Math.Pow ((double)random, 1d / (double)random);
			random = Math.Pow ((double)random, 1d / (double)random);*/
			/*random = Math.Log (random) / Math.Log (2);
			random = Math.Log (random) / Math.Log (2);*/

			int allChances = (int)Mathf.Pow (2, 4);

			int random = Random.Range (1, allChances);

            random = (int)System.Math.Log(random, 2);

            /*BigInteger n = new BigInteger((long)random);
			//n = random;

			int bitLength = 0;

			while (n / 2 != 0) {
				n /= 2;
				bitLength++;
			}
			bitLength += 1;

            random = bitLength;

            random--;*/
            /*if (random <= -1)
            {
                print("shit happened");
                break;
            }*/


            //random = (int)map(random, 0, allChances, 0, 4);
            int ourSize = random;
            ourSize = (-ourSize) + 4 - 1;
            int cust = (int)ourSize;
            /*if ((float)cust < (float)random - 0.5f)
				cust++;*/
            //cust++;
            //cust--;
            //random = cust;
            //print (random);
            //print ("-------------");
            if (cust != 0 && cust != 1 && cust != 2 && cust != 3 && cust != 4)
                Debug.LogError(cust);
            switch (cust) {
			case 0:
				zero++;
				break;
			case 1:
				one++;
				break;
			case 2:
				two++;
				break;
			case 3:
				three++;
				break;
			case 4:
				four++;
				break;
			default:
				wtf++;
				break;
			}

            if (cust != 0 && cust != 1 && cust != 2 && cust != 3 && cust != 4)
                print(cust);
            total++;
            AddData (cust);
		}
        calc = zero + one + two + three + four;
        print (zero + " - " + one + " - " + two + " - " + three + " - " + four + " - " + wtf + " - " + total + " - " + calc);
	}

    void Test2()
    {

        int zero = 0;
        int one = 0;
        int two = 0;
        int three = 0;
        int four = 0;
        int wtf = 0;
        int total = 0;
        int calc = 0;

        for (int i = 0; i <= 1000; i++)
        {
            fail:
            /*float allChances = Mathf.Pow (2, 4) - 1;
			allChances = Mathf.Pow (2, 4);
			allChances = Mathf.Pow (2, allChances);
			double random = (double)UnityEngine.Random.Range (0f, (float)allChances);
			//int ourSize = Mathf.ClosestPowerOfTwo (random);*/



            //print ("-------------");
            //print (allChances);
            //print (random);
            //print (Mathf.Sqrt (random));
            //random = cust;
            /*random = Mathf.Sqrt ((float)random);
			random = Mathf.Sqrt ((float)random);
			random /= 1;*/

            /*random = Math.Pow ((double)random, 1d / (double)random);
			random = Math.Pow ((double)random, 1d / (double)random);*/
            /*random = Math.Log (random) / Math.Log (2);
			random = Math.Log (random) / Math.Log (2);*/


            /*float allChances = Mathf.Pow(2, 4) - 1;

            float random = Random.Range(1f, (float)allChances);

            BigInteger n = new BigInteger((long)random);
            //n = random;

            int bitLength = 0;

            while (n / 2 != 0)
            {
                n /= 2;
                bitLength++;
            }
            bitLength += 1;

            random = bitLength;

            random--;*/
            /*if (random <= -1)
            {
                print("shit happened");
                break;
            }*/
            
            float allChances = Mathf.Pow (2, 4) - 1;
			allChances = Mathf.Pow (4, 2) -1 ;
			float random = Random.Range (0f, (float)allChances);
			//int ourSize = Mathf.ClosestPowerOfTwo (random);
        

            //random = (int)map(random, 0, allChances, 0, 4);
            int cust = Mathf.FloorToInt((1 + Mathf.Sqrt(1 + (8 * random)))/ 2);
            /*if ((float)cust < (float)random - 0.5f)
				cust++;*/
            //cust++;
            //cust--;
            //random = cust;
            //print (random);
            //print ("-------------");
            if (cust != 0 && cust != 1 && cust != 2 && cust != 3 && cust != 4)
                print(cust);
            switch (cust)
            {
                case 0:
                    zero++;
                    break;
                case 1:
                    one++;
                    break;
                case 2:
                    two++;
                    break;
                case 3:
                    three++;
                    break;
                case 4:
                    four++;
                    break;
                default:
                    wtf++;
                    break;
            }

            if (cust != 0 && cust != 1 && cust != 2 && cust != 3 && cust != 4)
                print(cust);
            total++;
            AddData(cust);
        }
        calc = zero + one + two + three + four;
        print(zero + " - " + one + " - " + two + " - " + three + " - " + four + " - " + wtf + " - " + total + " - " + calc);
    }

    void SetUpSliders(){

		sliders = new GameObject[resolution];

		for(int i = 0; i < sliders.Length; i++) {
			
			GameObject mySlider = (GameObject)Instantiate (sliderPrefab, transform.position, transform.rotation);
			mySlider.GetComponent<RectTransform> ().parent = sliderParent.GetComponent<RectTransform> ();
			mySlider.transform.localScale = new Vector3 (1, 1, 1);
			mySlider.GetComponentInChildren<Slider> ().maxValue = maxValue;

			sliders [i] = mySlider;
		}


	}
	
	// Update is called once per frame
	void Update () {
		int cust = (int)test;
		if ((float)cust < test - 0.5f)
			cust++;

		test2 = cust;
	}


	public void delayedStart(){
		//logAll ();
	}

	public void AddData (float data){

		int i = 0;
		while(data > valueRange.x){
			data -= (valueRange.y - valueRange.x) / resolution;

			i++;
		}

		sliders [i].GetComponentInChildren<Slider> ().value++;

	}

	float map (float x, float in_min, float in_max, float out_min, float out_max){

		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	/*void setBoolNest(){

		SetEachNest (body, s.body);
		SetEachNest (barrel, s.barrel);
		SetEachNest (magazine, s.magazine);
		SetEachNest (scope, s.scope);
		SetEachNest (stock, s.stock);

	}

	void SetEachNest(NestedGameObject[] obj, STORAGE_GunParts.NestedGameObject[] parent){

		obj = new NestedGameObject[parent.Length];

		for(int i = 0; i < parent.Length; i++) {

			obj[i].GunParts = new bool[parent[i].GunParts.Length];

			for(int ii = 0; ii < obj[i].GunParts.Length; ii++) {
				obj [i].GunParts [ii] = true;
			}
		}
	}*/



	//DONT USE THIS IT TAKES AN ETERNITY



	/*public void logAll (){

		float time = Time.time;
		//Body Level
		for (int bodyLevel = b.myGun.gunRarity; bodyLevel < b.myGun.gunLevel; bodyLevel++) {
			//body type
			for(int bodyType = 0; bodyType < STORAGE_GunParts.s.body[bodyLevel].GunParts.Length; bodyType++){
				//if (!body [bodyLevel].GunParts [bodyType])
					//break; //break if that part is disabled

				b.myGun.bodyLevel = bodyLevel;
				b.myGun.bodyType = bodyType;

				//barrel level
				for (int barrelLevel = b.myGun.gunRarity; barrelLevel < b.myGun.gunLevel; barrelLevel++) {
					//barrel type
					for(int barrelType = 0; barrelType < STORAGE_GunParts.s.barrel[barrelLevel].GunParts.Length; barrelType++){
						//if (!barrel [barrelLevel].GunParts [barrelType])
							//break; //break if that part is disabled

						b.myGun.barrelLevel = barrelLevel;
						b.myGun.barrelType = barrelType;

						//Magazine level
						for (int magazineLevel = b.myGun.gunRarity; magazineLevel < b.myGun.gunLevel; magazineLevel++) {
							//Magazine type
							for(int magazineType = 0; magazineType < STORAGE_GunParts.s.magazine[magazineLevel].GunParts.Length; magazineType++){
								//if (!barrel [magazineLevel].GunParts [magazineType])
									//break; //break if that part is disabled

								b.myGun.magazineLevel = magazineLevel;
								b.myGun.magazineType = magazineType;

								//scope level
								for (int scopeLevel = b.myGun.gunRarity; scopeLevel < b.myGun.gunLevel; scopeLevel++) {
									//scope type
									for(int scopeType = 0; scopeType < STORAGE_GunParts.s.scope[scopeLevel].GunParts.Length; scopeType++){
										//if (!barrel [scopeLevel].GunParts [scopeType])
											//break; //break if that part is disabled

										b.myGun.scopeLevel = scopeLevel;
										b.myGun.scopeType = scopeType;

										//stock level
										for (int stockLevel = b.myGun.gunRarity; stockLevel < b.myGun.gunLevel; stockLevel++) {
											//stock type
											for(int stockType = 0; stockType < STORAGE_GunParts.s.stock[stockLevel].GunParts.Length; stockType++){
												//if (!barrel [stockLevel].GunParts [stockType])
													//break; //break if that part is disabled

												b.myGun.stockLevel = stockLevel;
												b.myGun.stockType = stockType;

												//FINAL
												b.BuildGun();
												yield return 0;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		print ("DONE");
		StartCoroutine ("timeLogger", time);
	}

	IEnumerator timeLogger(float time){
		yield return 0;
		print(Time.time - time);
	}*/
}
