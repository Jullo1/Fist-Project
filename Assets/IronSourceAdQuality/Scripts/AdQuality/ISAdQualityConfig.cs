using System;
using UnityEngine;

public class ISAdQualityConfig
{
	private String userId;
	private bool userIdSet;
	private bool testMode;
	private ISAdQualityInitCallback adQualityInitCallback;
	private ISAdQualityLogLevel logLevel;
	private String initializationSource;
	private bool coppa;
	private ISAdQualityDeviceIdType deviceIdType;

	public ISAdQualityConfig()
	{
		userId = null;
		testMode = false;
		userIdSet = false;
		logLevel = ISAdQualityLogLevel.INFO;
		coppa = false;
		deviceIdType = ISAdQualityDeviceIdType.NONE;
		initializationSource = null;
	}

	public String UserId 
	{
		get
		{
			return userId;
		}
		set 
		{
			userIdSet = true;
			userId = value;
		}
	}
	
	internal bool UserIdSet 
	{
		get 
		{
			return userIdSet;
		}	
	}
	
	public bool TestMode 
	{
		get
		{
			return testMode;
		} 
		set 
		{
			testMode = value;
		}
	}

	public ISAdQualityLogLevel LogLevel
	{
		get 
		{
			return logLevel;
		}
		set
		{
			logLevel = value;
		}
	}

	public ISAdQualityInitCallback AdQualityInitCallback 
	{
		get 
		{
			return adQualityInitCallback;
		} 
		set 
		{
			adQualityInitCallback = value;
		}
	}

	public String InitializationSource
	{
		get
		{
			return initializationSource;
		}
		set
		{
			initializationSource = value;
		}
	}

	public bool Coppa
	{
		get
		{
			return coppa;
		}
		set
		{
			coppa = value;
		}
	}

	public ISAdQualityDeviceIdType DeviceIdType
	{
		get
		{
			return deviceIdType;
		}
		set
		{
			deviceIdType = value;
		}
	}
}