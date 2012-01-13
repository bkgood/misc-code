from afap.util.rangeddate import RangedDate
from django.template import Context
from django.template.loader import get_template
from afap.models import Application
from django.contrib.auth.models import User
nonprov = get_template('emails/import.txt')
prov = get_template('emails/import_provisional.txt')

def gen():
    emails=[]
    for app in Application.objects.filter(year=2011):
        password = User.objects.make_random_password()
        app.organization.user.set_password(password)
        app.organization.user.save()
        if app.provisional:
            t=prov
        else:
            t=nonprov
        emails.append(app.email("2011-2012 AFAP Application", t.render(Context({'organization':app.organization,'username':app.organization.user.username,'password':password,
         'year':RangedDate(2011,1,1), 'constituency_group':app.organization.constituency_group.name})), 'EMAILREMOVED', True))
    return emails

