using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelUI : MonoBehaviour
{

	public ClientStartUp clientStartUp;
	public ServerStartUp serverStartUp;

	public Button loginButton;
	public Button startLocalServerButton;

    void Start()
    {
		loginButton.onClick.AddListener(clientStartUp.OnLoginUserButtonClick);
		startLocalServerButton.onClick.AddListener(serverStartUp.OnStartLocalServerButtonClick);
	}
}
