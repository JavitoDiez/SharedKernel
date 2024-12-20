﻿
namespace PayPal.V1.Shared.ModelsToMigrate;

/// <summary>
/// The cost as a percent or an amount value. For example, to specify 10%, enter `10`. Alternatively, to specify an amount of 5, enter `5`.
/// <para>
/// See <a href="https://developer.paypal.com/docs/api/">PayPal Developer documentation</a> for more information.
/// </para>
/// </summary>
public class Cost //: PayPalSerializableObject
{
    /// <summary>
    /// The cost, as a percent value. Valid value is from 0 to 100.
    /// </summary>
    public float Percent { get; set; }

    /// <summary>
    /// The cost, as an amount value. Valid value is from 0 to 1,000,000.
    /// </summary>
    public PayPalCurrency? Amount { get; set; }
}