->PoliceStart

=== PoliceStart ===
#speaker Brian the NYPD Officer
How's your day, pal?

+[Good] ->goodDay
+[Bad] ->badDay

=== goodDay ===
#speaker You
It's been good!

#speaker You
You?

#speaker Brian the NYPD Officer
I'm glad to hear that!

#speaker Brian the NYPD Officer
My day hasn't been going so well.

#speaker Brian the NYPD Officer
I lost my police badge...

+[I can help you find it!] ->helpOut
+[I hope you find it soon.] ->dontHelp

=== badDay ===
#speaker You
It's hasn't been great.

#speaker Brian the NYPD Officer
Sorry to hear that.

#speaker You
What about you?

#speaker Brian the NYPD Officer
Not much better

#speaker Brian the NYPD Officer
My day hasn't been going so well either.

#speaker Brian the NYPD Officer
I lost my police badge...

+[I can help you find it!] ->helpOut
+[I hope you find it soon.] ->dontHelp

=== helpOut ===
#speaker You
I can help you find it!

#speaker Brian the NYPD Officer
That'd be great.

#speaker Brian the NYPD Officer
I must have lost it while doing rounds.

#speaker Brian the NYPD Officer
Just keep an eye out for me, okay?

#speaker Brian the NYPD Officer
I'll stay at my post in the meantime.
->END

=== dontHelp ===
#speaker You
I hope you find it soon.

#speaker Brian the NYPD Officer
Thanks, pal.

->END

