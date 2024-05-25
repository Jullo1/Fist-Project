using System;

public enum ISAdQualityInitError {
    AD_QUALITY_SDK_WAS_SHUTDOWN                     = 0,
    ILLEGAL_USER_ID                                 = 1,
    ILLEGAL_APP_KEY                                 = 2,
    EXCEPTION_ON_INIT                               = 3,
    NO_NETWORK_CONNECTION                           = 4,
    CONFIG_LOAD_TIMEOUT                             = 5,
    CONNECTOR_LOAD_TIMEOUT                          = 6,
    AD_NETWORK_VERSION_NOT_SUPPORTED_YET            = 7,
    AD_NETWORK_SDK_REQUIRES_NEWER_AD_QUALITY_SDK    = 8,
    AD_QUALITY_ALREADY_INITIALIZED                  = 9
};