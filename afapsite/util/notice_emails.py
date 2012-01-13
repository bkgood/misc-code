#!/usr/bin/env python2

from afap.util.rangeddate import RangedDate
from django.template import Context
from django.template.loader import get_template
from afap.models import Application

from django.core import mail

template = get_template('emails/incomplete.txt')

#no_budget = [
#    app for app in Application.objects.filter(year=2011, provisional=False)
#    if app.has_org_info() and not app.has_budget_request()]
#print "no budget:"
#print no_budget
#approved_negative = [
#    app for app in Application.objects.filter(year=2011, provisional=False)
#    if app.has_budget_request() and app.request_amount() < 0 and app.approved()]
#print "\napproved, negative:"
#print approved_negative


def gen():
    budget_negative_missing_sigs = [
        app for app in Application.objects.filter(year=2011, provisional=False)
        if app.has_budget_request() and app.request_amount() < 0 and not app.approved()]
    missing_sigs = [
        app for app in Application.objects.filter(year=2011, provisional=False)
        if app.has_org_info() and app.has_budget_request()
        and app.request_amount() > 0 and not app.approved()]
    emails=[]
    for app in budget_negative_missing_sigs:
        email = app.email("Incomplete AFAP Application (%s)" % (app.organization.name),
            template.render(
                Context({'organization':app.organization, 'negative_request': True})), 'EMAILREMOVED', True)
        emails.append((app.organization.name, mail.EmailMessage(*email)))
    for app in missing_sigs:
        email = app.email("Incomplete AFAP Application (%s)" % (app.organization.name),
            template.render(
                Context({'organization':app.organization, 'negative_request': False})), 'EMAILREMOVED', True)
        emails.append((app.organization.name, mail.EmailMessage(*email)))
    return emails

def send_emails():
    emails = gen()
    #for (org, email) in emails: print email.message()
    #conn = mail.get_connection()
    #conn.open()
    for (org, email) in emails:
        print "sending email to %s" % (org,)
        #conn.send_messages((email,))
    #conn.close()


if __name__ == '__main__': send_emails()
