import afap.models
from afap.util.rangeddate import RangedDate
import csv
import sys

YEAR = RangedDate(2011, 1, 1)
FILENAME = "/home/bill/Feb. AFAP eligible list.csv"

def import_const(reader, const_group):
    groups = 0
    print "Constituency %s" % (const_group,)
    reader.next() # skip header
    for row in reader:
        row = [entry.strip() for entry in row]
        if row[0] == '':
            return groups
        (org, created) = afap.models.Organization.objects.get_or_create(name=row[0], constituency_group=const_group,
            account_number=row[7])
        print "Organization %s" % org
        (app, created) = afap.models.Application.objects.get_or_create(organization=org, year=YEAR)
        (president, treasurer, advisor) = (None, None, None)
        if row[3] and row[4]:
            (president, created) = afap.models.Person.objects.get_or_create(name=row[3], email_address=row[4])
        if row[5] and row[6]:
            (treasurer, created) = afap.models.Person.objects.get_or_create(name=row[5], email_address=row[6])
        if row[1] and row[2]:
            (advisor, created) = afap.models.Person.objects.get_or_create(name=row[1], email_address=row[2])
        (app.president, app.treasurer, app.advisor) = (president, treasurer, advisor)
        app.provisional = not row[9]
        app.save()
        groups += 1
    return groups


def import_organizations():
    csvfile = open(FILENAME, 'rb')
    dialect = csv.Sniffer().sniff(csvfile.read())
    csvfile.seek(0)
    reader = csv.reader(csvfile, dialect)
    for row in reader:
        row = [entry.strip() for entry in row]
        (const_group, created) = afap.models.ConstituencyGroup.objects.get_or_create(name=row[0])
        groups = import_const(reader, const_group)
        print '%s: %d groups' % (const_group, groups)
    csvfile.close()

def main():
    import_organizations(sys.argv[1])

if __name__ == '__main__': main()
