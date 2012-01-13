from afap.models import Application
from datetime import datetime, timedelta

from django.template import Context
from django.template.loader import get_template
from afap.models import Application

from django.core import mail

template = get_template('emails/hearing.txt')


format_string = "%A, %d %B, %Y at %I:%M%p"

START_TIME = datetime(2011, 4, 20, 16, 05)
DELTA = timedelta(minutes=5)
LOCATION = "SU 250 (Council Room)"

def schedule():
    elligible = [app for app in Application.objects.filter(year=2011,
        provisional=False,
        organization__constituency_group__name__startswith="College of Engin").order_by('organization__name')
        if app.approved()]

    for x in xrange(len(elligible)):
        if "Agricultural" in elligible[x].organization.name:
            app = elligible.pop(x)
            elligible.insert(11, app)

    # generate emails
    emails = []
    time = START_TIME
    for app in elligible:
        print "%s %s %s" % (app.organization, time.date().isoformat(), time.time().isoformat())
        email = app.email("AFAP Hearing (%s)" % (app.organization.name),
            template.render(
                Context({'organization':app.organization, 'datetime': time, 'location': LOCATION,
                    'constituency_group': app.organization.constituency_group})),
            'EMAILREMOVED', True)
        emails.append((app.organization.name, mail.EmailMessage(*email)))
        time += DELTA

    print

    for (org, email) in emails: print email.message()

    print

    conn = mail.get_connection()
    conn.open()
    for (org, email) in emails:
        print "sending email to %s" % (org,)
        conn.send_messages((email,))
    conn.close()

if __name__ == '__main__': schedule()
