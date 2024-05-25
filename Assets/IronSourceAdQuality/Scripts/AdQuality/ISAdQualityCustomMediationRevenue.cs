using System;
using UnityEngine;

public class ISAdQualityCustomMediationRevenue
{
    private ISAdQualityMediationNetwork mediationNetwork;
    private ISAdQualityAdType adType;
    private double revenue;
    private String placement;

	public ISAdQualityCustomMediationRevenue()
	{
		mediationNetwork = ISAdQualityMediationNetwork.UNKNOWN;
		adType = ISAdQualityAdType.UNKNOWN;
		revenue = 0;
		placement = null;
	}

	public ISAdQualityMediationNetwork MediationNetwork
	{
		get
		{
			return mediationNetwork;
		}
		set 
		{
			mediationNetwork = value;
		}
	}
	
	public ISAdQualityAdType AdType 
	{
		get
		{
			return adType;
		} 
		set 
		{
			adType = value;
		}
	}

	public double Revenue
	{
		get 
		{
			return revenue;
		}
		set
		{
			revenue = value;
		}
	}

	public String Placement 
	{
		get 
		{
			return placement;
		} 
		set 
		{
			placement = value;
		}
	}
}