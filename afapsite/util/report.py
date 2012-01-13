#!/usr/bin/env python2

import csv
import sys
from afap.models import ConstituencyGroup
from afap.models import Application
from afap.models import Allocation

def gen_report():
#    f = open('report.csv', 'wb')
    w = csv.writer(sys.stdout)
    w.writerow(['Organization', 'Account Number', 'Allocation', 'Appeal'])
    constituencies = ConstituencyGroup.objects.order_by('name')
    for c in constituencies:
        apps = Application.objects.filter(year=2011, organization__constituency_group=c).order_by('organization__name')
        if len(apps) is 0:
            continue
        w.writerow([c.name])
        for a in apps:
            allocs = [alloc.amount for alloc in Allocation.objects.filter(application=a).order_by('-description')]
            if len(allocs) is 0:
                continue
            w.writerow([a.organization.name, a.organization.account_number] + allocs)
        w.writerow([])
            

    return

if __name__ == '__main__': gen_report()
