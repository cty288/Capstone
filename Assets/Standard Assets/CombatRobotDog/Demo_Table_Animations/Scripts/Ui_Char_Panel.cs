using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//creates the buttons on panel of animations
public class Ui_Char_Panel : MonoBehaviour {

	public GameObject character;
	public GameObject character_mat;
	public Transform acts_table;
 
    public Button buttonPrefab;

    Button sel_btm;

	Actions actions;

	public Transform mat_table;
 
    public Material[] Materials;
	void Start () {
 
		actions = character.GetComponent<Actions> ();
 


		CreateActionButton("Idle1");
		CreateActionButton("Idle2");
		CreateActionButton("Walk");
		CreateActionButton("Trot");
		CreateActionButton("Canter");		
		CreateActionButton("Jump");		
		CreateActionButton("Lame");
		CreateActionButton("StrafeLeft");
		CreateActionButton("StrafeRight");
		CreateActionButton("TurnLeft");
		CreateActionButton("TurnRight");


		CreateActionButton("Hit1");
		CreateActionButton("Hit2");
		CreateActionButton("Dead1");
		CreateActionButton("Dead2");
		CreateActionButton("Attack");
		CreateActionButton("StopAttack");
		CreateActionButton("PowerOn");
		CreateActionButton("PowerOff");

        CreateMatButton("Mat1", 0);
        CreateMatButton("Mat2", 1);
        CreateMatButton("Mat3", 2);
        CreateMatButton("Mat4 any texture", 3);
        CreateMatButton("Mat5 any texture", 4); 
        CreateMatButton("Mat6 any color", 5); 
 
    }

    void CreateMatButton(string name, int mat_n)
    {

        Button button = CreateButton(name, mat_table);
        button.GetComponentInChildren<Image>().color = new Color(.3f, .3f, .5f);
        button.GetComponentInChildren<Text>().fontSize = 12;
        button.onClick.AddListener(() => ButtonClicked(mat_n));


    } 
    void ButtonClicked(int mat_n)
    {
   
		character_mat.GetComponent<SkinnedMeshRenderer>().material = Materials[mat_n];
 
    }
    void CreateActionButton(string name) {

		CreateActionButton(name, name);
	}

 
	void CreateActionButton(string name, string message) {

		Button button = CreateButton (name, acts_table);
 
		if (name == "Idle1")
		{
			sel_btm = button;
			button.GetComponentInChildren<Image>().color = new Color(.5f, .5f, .5f);
		}
		button.GetComponentInChildren<Text>().fontSize = 12;
		button.onClick.AddListener(()  => actions.SendMessage(message, SendMessageOptions.DontRequireReceiver));
		button.onClick.AddListener(() => select_btm(button)  );


	}
    void select_btm(Button btm)
    {
		sel_btm.GetComponentInChildren<Image>().color = new Color(.345f, .345f, .345f);
		btm.GetComponentInChildren<Image>().color = new Color(.5f, .5f, .5f);
        sel_btm = btm;
    }

 
    Button CreateButton(string name, Transform group) {
		GameObject obj = (GameObject) Instantiate (buttonPrefab.gameObject);
		obj.name = name;
		obj.transform.SetParent(group);
		obj.transform.localScale = Vector3.one;
		Text text = obj.transform.GetChild (0).GetComponent<Text> ();
		text.text = name;
		return obj.GetComponent<Button> ();
	}



 
	public void OpenPublisherPage() {
		Application.OpenURL ("https://www.artstation.com/sintetus");
	}
 
 
}
