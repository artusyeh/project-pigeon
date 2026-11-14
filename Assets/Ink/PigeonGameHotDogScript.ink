->hotdog_start
=== hotdog_start ===
#speaker Dan the Hot Dog Man
Aw, man.

#speaker Dan the Hot Dog Man
My hot dog buns!

#speaker Dan the Hot Dog Man
I can't believe I lost them.

#speaker Dan the Hot Dog Man
How will I open business now?

+[Offer to Help]->helpHotDog
+[Leave]->leaveHotDog

=== helpHotDog ===
#speaker You
I can help!

#speaker Dan the Hot Dog Man
Really?

#speaker Dan the Hot Dog Man
Thank you. I appreciate that.

#speaker Dan the Hot Dog Man
I probably dropped them somewhere on the north side of the park.

#speaker Dan the Hot Dog Man
If you find them, I'll grill you the first hot dog of the day.

#speaker Dan the Hot Dog Man
How about that?

+[Accept] ->acceptTask
+[Reject] ->rejectTask

=== acceptTask ===
#speaker You
That sounds good to me.

#speaker Dan the Hot Dog Man
Great! I'll be waiting right here.
->END

=== rejectTask ===
#speaker You
Nevermind, I can't help right now.

#speaker Dan the Hot Dog Man
No worries, man.

#speaker Dan the Hot Dog Man
See ya.

->END

=== leaveHotDog ===
#speaker Narrator
You decided to leave the hot dog stand.

-> END


