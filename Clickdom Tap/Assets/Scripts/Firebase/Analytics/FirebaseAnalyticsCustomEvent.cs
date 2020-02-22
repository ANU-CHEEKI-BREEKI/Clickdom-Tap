using Firebase.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FirebaseAnalyticsCustomEvent
{
    public string Name { get; set; }

    public virtual bool CanLogEvent()
    {
        return FirebaseAnalyticsWrapper.Initialited;
    }

    public void LogEvent()
    {
        if (!CanLogEvent())
            return;
        Log();
    }

    protected virtual void Log()
    {
        FirebaseAnalytics.LogEvent(Name);
    }
}

public class FirebaseAnalyticsCustomParameteredEvent : FirebaseAnalyticsCustomEvent
{
    public Parameter[] Parameters { get; set; }

    protected override void Log()
    {
        FirebaseAnalytics.LogEvent(Name, Parameters);
    }
}
