#!/usr/bin/env python2

from afap.models import Application
from afap.models import Allocation

import csv
import sys

def main(args):
    csvfile = open(args[1], 'r')
    dialect = csv.Sniffer().sniff(csvfile.read())
    csvfile.seek(0)
    reader = csv.reader(csvfile, dialect)
    allocations = []
    for row in reader:
        row = [entry.strip() for entry in row]
        app = Application.objects.get(pk=row[0])
        print "%s %s" % (app.organization.name, row[2])
        allocations.append(Allocation(application=app, description="Appeals",
                amount=row[2]))
    csvfile.close()
    for x in allocations:
        x.save()

if __name__ == '__main__': main(sys.argv)
