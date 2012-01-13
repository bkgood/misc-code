import datetime
import sha # deprecated since 2.5 but since the web host has 2.4...
import re

from django.db import models
from django import forms
from django.contrib.auth.models import User
import django.contrib.localflavor.us.models as usmodels
from django.core import validators, mail
from django.template import Context, Template
from django.template.loader import render_to_string, get_template
import afap.formfields
import afap.modelfields
from util import sum_line_items

# The number of digits (before and after decimal point) to store in decimal
# numbers. 100,000.00 is 8 digits, so 10 should be plenty
DECIMAL_DIGITS = 10
# the number of decimal places to keep track of. Decimal values will almost
# always be currency, so 2 is what we want
DECIMAL_PLACES = 2
# used where we don't particularly care how long the field is, just as long
# as possible without using a TextField
CHARFIELD_LENGTH = 255

class Year(models.Model):
    '''Sets various options for a year of AFAP hearings.'''
    date = afap.modelfields.YearField(unique=True, selectable=False, verbose_name="Year number")
    active = models.BooleanField(default=False)
    email_from = models.EmailField()
    read_only = models.BooleanField(default=True)
    validation_enabled = models.BooleanField(default=True)

    year = property(lambda self: self.date.year)

    range = property(lambda self: self.date.range)

    class Meta(object):
        get_latest_by = 'date'
        ordering = ['date']

    def __unicode__(self):
        return u'%d' % (self.year,)

    @classmethod
    def current_year(cls):
        return cls.objects.filter(active=True).aggregate(models.Max('date')).get('date__max')

    def save(self):
        if self.active:
            try:
                temp = Year.objects.get(active=True)
                if self != temp:
                    temp.active = False
                    temp.save()
            except Year.DoesNotExist:
                pass
        super(Year, self).save()

class Person(models.Model):
    '''A person who has a role in an organization and can be sent email.'''
    name = models.CharField(max_length=CHARFIELD_LENGTH)
    email_address = models.EmailField()
    phone_number = usmodels.PhoneNumberField(blank=True)

    class Meta(object):
        verbose_name_plural = 'people'
        ordering = ('name',)


    def __unicode__(self):
        if self.phone_number:
            return u'%s <%s> (%s)' % (self.name, self.email_address, self.phone_number)
        else:
            return u'%s <%s>' % (self.name, self.email_address,)

    def email(self, subject, message, from_, tuple_=False):
        if tuple_:
            return (subject, message, from_, [self.email_address,])
        else:
            mail.send_mail(subject, message, from_, [self.email_address,], fail_silently=False)
        return

    def detailed_address(self):
        return u'"%s" <%s>' % (self.name, self.email_address,)

class ConstituencyGroup(models.Model):
    '''A grouping which an organization belongs to.'''
    name = models.CharField(max_length=CHARFIELD_LENGTH)

    def __unicode__(self):
        return u'%s' % (self.name,)

class Organization(models.Model):
    '''A student organization able to apply for funding.'''
    user = models.ForeignKey(User, null=True, blank=True)
    name = models.CharField(max_length=CHARFIELD_LENGTH)
    constituency_group = models.ForeignKey(ConstituencyGroup)
    account_number = models.CharField(max_length=CHARFIELD_LENGTH, null=True, blank=True)
    # used to disable groups that were once active but are no longer
    # (and may later become active)
    active = models.BooleanField(default=True)

    class Meta:
        ordering = ('name',)

    def __unicode__(self):
        return u'%s' % (self.name,)

    def link_user(self):
        password = User.objects.make_random_password()
        self.user = User.objects.create_user(self._generate_username(), '', password)
        self.user.set_unusable_password()
        self.save()
        return password

    def _generate_username(self):
        # nuke: club, organization, oklahoma, state, university, punctuation,
        # anything in parens, 
        nuke_whitespace = re.compile(r'''\W''')
        no_parens = re.compile(r'''\([^)]+\)''')
        filtered_words = '''club student association organization council for in
            society collegiate of the osu and oklahoma state university assoc student
            america national team institute'''.split()
        word_filter = re.compile(r'''\b(%s)\b''' % ('|'.join(filtered_words),))
        name = self.name.lower()
        name = nuke_whitespace.sub("", word_filter.sub("", no_parens.sub("", name)))
        if len(name) > 30: # max length of username
            name = name[:25]
            name = '%s%d' % (name, self.pk % 100000) # if there are ever collisions with
            # this then god help us
        return name

class Application(models.Model):
    organization = models.ForeignKey(Organization, unique_for_year='year')
    year = afap.modelfields.YearField()
    president = models.ForeignKey(Person, related_name='president', null=True, blank=True)
    email_president = models.BooleanField(default=True)
    treasurer = models.ForeignKey(Person, related_name='treasurer', null=True, blank=True)
    email_treasurer = models.BooleanField(default=True)
    advisor = models.ForeignKey(Person, related_name='advisor', null=True, blank=True)
    email_advisor = models.BooleanField(default=True)
    members = models.IntegerField("number of members", default=0,
            validators=[validators.MinValueValidator(0)])
    dues = models.DecimalField(default='0', max_digits=DECIMAL_DIGITS,
            decimal_places=DECIMAL_PLACES)
    new_members = models.IntegerField("anticipated new members", default=0)
    purpose = models.TextField(blank=True)
    membership_requirements = models.TextField(blank=True)
    note = models.TextField("notes", blank=True)
    last_modified = models.DateTimeField(blank=True)
    provisional = models.BooleanField(default=False)
    balance_forward = models.DecimalField("Balance forward", max_digits=DECIMAL_DIGITS,
            decimal_places=DECIMAL_PLACES, default=0)
    afap_income = models.DecimalField("AFAP Income", max_digits=DECIMAL_DIGITS,
            decimal_places=DECIMAL_PLACES, default=0,
            validators=[validators.MinValueValidator(0)])

    def __unicode__(self):
        return u'Application for %s (%d)' % (self.organization.name,
                self.year.year,)

    def _year_for_admin(self):
        return self.year.year
    _year_for_admin.admin_order_field = 'year'
    _year_for_admin.short_description = 'year'

    def has_org_info(self):
        return (self.members > 0 or self.dues > 0 or self.new_members != 0
            or self.purpose != '' or self.membership_requirements != '')

    def has_budget_request(self):
        return (self.lineitem_set.exists() or self.balance_forward != 0 or
            self.afap_income != 0)

    def approved(self):
        for approval in self.approval_set.all():
            if not approval.approved:
                return False
        return True

    def request_amount(self):
        sums = []
        for category in LineItemCategory.objects.filter(years__date=self.year):
            sums.append(sum_line_items(
                LineItem.objects.filter(application=self, category=category)))
        total_sum = sum(sums) - self.balance_forward - self.afap_income
        total_sum = total_sum or 0
        return total_sum
        
    # look into only updating last_modified when the user makes a chance,
    # not when an admin does
    def save(self, *args, **kwargs):
        '''On save, update last_modified'''
        self.last_modified = datetime.datetime.now()
        approvers = (self.president, self.treasurer, self.advisor)
        for approval in self.approval_set.all():
            if approval.approver not in approvers:
                approval.delete()
            else:
                approval.approved = False
                approval.approved_at = None
                approval.generate_key()
                approval.save()
        for person in [person for person
                in (self.president, self.treasurer, self.advisor)
                if person is not None]:
            try:
                self.approval_set.get(approver=person)
            except (Approval.DoesNotExist):
                Approval(application=self, approver=person).save()
        return super(Application, self).save(*args, **kwargs)

    def print_to_pdf(self, pdf):
        return

    def organization_constituency(self):
        return self.organization.constituency_group
    organization_constituency.description = "the applying organization's const"

    def email(self, subject, message, from_, tuple_=False):
        recps = []
        if self.president and self.email_president:
            recps.append(self.president.detailed_address())
        if self.treasurer and self.email_treasurer:
            recps.append(self.treasurer.detailed_address())
        if self.advisor and self.email_advisor:
            recps.append(self.advisor.detailed_address())
        if tuple_:
            return (subject, message, from_, recps)
        else:
            mail.send_mail(subject, message, from_, recps, fail_silently=False)
        return


class ApplicationFile (models.Model):
    application = models.ForeignKey(Application)
    file = models.FileField(upload_to='applicationfiles/%Y/%S/')
    replaces_application = models.BooleanField(default=True)

    def __unicode__(self):
        return u'File for application %s' % unicode(self.application)

class LineItemCategory(models.Model):
    '''A line item category.'''
    name = models.CharField(max_length=CHARFIELD_LENGTH)
    description = models.TextField(blank=True)
    years = models.ManyToManyField(Year)

    class Meta(object):
        verbose_name_plural = 'line item categories'

    def __unicode__(self):
        return self.name

class LineItem(models.Model):
    '''An income or expense item.'''
    application = models.ForeignKey(Application)
    category = models.ForeignKey(LineItemCategory)
    description = models.CharField(max_length=CHARFIELD_LENGTH)
    amount = models.DecimalField("Amount ($)", max_digits=DECIMAL_DIGITS,
            decimal_places=DECIMAL_PLACES,
            validators=[validators.MinValueValidator(0)])
    income = models.BooleanField(default=False)

    def __unicode__(self):
        return u'Line item %d' % (self.id,)

    def save(self, *args, **kwargs):
        '''On save, update Application.last_modified'''
        #self.application.last_modified = datetime.datetime.now()
        self.application.save()
        return super(LineItem, self).save(*args, **kwargs)

class Approval(models.Model):
    '''An approval of an application by a person.'''
    approver = models.ForeignKey(Person)
    application = models.ForeignKey(Application)
    approved = models.BooleanField(default=False)
    key = models.CharField(max_length=40, blank=True) # sha1 hash used in approval urls
    approved_at = models.DateTimeField(blank=True, null=True)
    notes = models.TextField(blank=True)

    def __unicode__(self):
        return 'Approval by %s for %s' % (self.approver.name,
                self.application.organization.name,)

    def _roles(self):
        roles = list()
        if self.approver == self.application.president:
            roles.append('president')
        if self.approver == self.application.treasurer:
            roles.append('treasurer')
        if self.approver == self.application.advisor:
            roles.append('advisor')
        return ', '.join(roles)
    roles = property(_roles)

    def generate_key(self):
        hasher = sha.new()
        # hash uses the approver, current date+time and the organization name
        # and year of the application. relatively weak but the now() method
        # seems to give fairly precise timing in Linux so hopefully strong
        # enough
        hasher.update(str(self.approver))
        hasher.update(str(datetime.datetime.now()))
        hasher.update(str(self.application))
        self.key = hasher.hexdigest()
        return

    def save(self, *args, **kwargs):
        if self.approved:
            self.approved_at = datetime.datetime.now()
        else:
            self.approved_at = None
        super(Approval, self).save(*args, **kwargs)

    def email_key(self):
        t = get_template('emails/approve.txt')
        c = Context({'to': self.approver.name, 'organization': self.application.organization.name,
            'year': self.application.year, 'hash': self.key,})
        from_ = Year.objects.get(date=self.application.year).email_from
        self.approver.email("AFAP Application Approval", t.render(c), from_)
        return

class Allocation (models.Model):
    application = models.ForeignKey(Application)
    description = models.CharField(max_length=CHARFIELD_LENGTH)
    amount = models.DecimalField("Amount ($)", max_digits=DECIMAL_DIGITS,
            decimal_places=DECIMAL_PLACES,
            validators=[validators.MinValueValidator(0)])
    notified = models.BooleanField(default=False)

    def __unicode__(self):
        return "Allocation '%s' ($%d) for %s (%d)" % (self.description,
                self.amount, self.application.organization.name,
                self.application.year.year,)
