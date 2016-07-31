using System;

/// <summary>
/// A Low-Pass Filter for reducing noise by increasing lag and decreasing sensitivity.
/// <remarks>Technical explaination: http://en.wikipedia.org/wiki/Low-pass_filter</remarks>
/// </summary>
public class LowPassFilter
{
    private double m_Top;
    private double m_Cutoff;
    private double m_Value;

    /// <summary>
    /// The filtered value.
    /// </summary>
    public double Value
    {
        get
        {
            return m_Value;
        }
    }

    /// <summary>
    /// Create an instance of a Low Pass Filter, which removes noise in favour of lag.
    /// </summary>
    /// <param name="cutoff">The amount of cutoff</param>
    public LowPassFilter(double cutoff) : this(cutoff, 0) { }

    /// <summary>
    /// Create an instance of a Low Pass Filter, which removes noise in favour of lag.
    /// </summary>
    /// <param name="cutoff">The amount of cutoff</param>
    /// <param name="initialState">The initial state of the filter</param>
    public LowPassFilter(double cutoff, double initialState)
    {
        if (cutoff > 0.5)
        {
            throw new ArgumentOutOfRangeException("cutoff should be less than 0.5");
        }
        m_Cutoff = cutoff;
        m_Top = 1 - m_Cutoff;

        m_Value = initialState;
    }

    /// <summary>
    /// Update the filter
    /// </summary>
    /// <param name="value">The new value from the input</param>
    /// <returns>The filtered value</returns>
    public double Update(double value)
    {
        m_Value = (m_Value * m_Top) + (value * m_Cutoff);
        return m_Value;
    }
}
