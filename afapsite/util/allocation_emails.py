#!/usr/bin/env python2

from afap.util.rangeddate import RangedDate
from django.template import Context
from django.template.loader import get_template
from afap.models import Allocation

from django.core import mail
from django.db.models import Sum

template = get_template('emails/allocation.txt')

def gen():
    emailed = [] # list of applications
    new_allocations = Allocation.objects.filter(application__year__year=2011,
            notified=False)
    emails = []

    for allocation in new_allocations:
        if allocation.application in emailed:
            continue
        org = allocation.application.organization
        allocations = Allocation.objects.filter(application=allocation.application).order_by('pk')
        allocations_sum = allocations.aggregate(Sum('amount'))['amount__sum']
        year = allocation.application.year
        email = allocation.application.email("AFAP Allocation (%s)" % (org.name,),
            template.render(
                Context({'organization': org, 'allocations': allocations,
                    'sum': allocations_sum, 'year': year,})), 'MYEMAIL', True)
        emails.append((org.name, mail.EmailMessage(*email)))
        emailed.append(allocation.application)
    new_allocations.update(notified=True)
    return emails

def send_emails():
    emails = gen()
    for (org, email) in emails: print email.message()
    conn = mail.get_connection()
    conn.open()
    for (org, email) in emails:
        print "sending email to %s" % (org,)
        conn.send_messages((email,))
    conn.close()


if __name__ == '__main__': send_emails()
