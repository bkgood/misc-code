from django import forms
from django.contrib import auth
import django.contrib.auth.forms
from django.db.models.fields import BLANK_CHOICE_DASH
from django.core.exceptions import ObjectDoesNotExist, MultipleObjectsReturned

import afap.models

class ApplicationForm (forms.ModelForm):
    class Meta:
        model = afap.models.Application
        exclude = ('organization', 'year', 'president', 'treasurer', 'advisor',
            'email_president', 'email_treasurer', 'email_advisor', 'last_modified', 'balance_forward',
            'afap_income', 'provisional',)

class ContactForm (forms.ModelForm):
    class Meta:
        model = afap.models.Application
        fields = ('email_president','email_treasurer', 'email_advisor')

class LineItemForm (forms.ModelForm):
    class Meta:
        model = afap.models.LineItem
        fields = ('description', 'amount')

class ApprovalForm (forms.ModelForm):
    class Meta:
        model = afap.models.Approval
#        fields = ('approved', 'notes')
        fields = ('approved',)

class PhoneNumberForm (forms.ModelForm):
    class Meta:
        model = afap.models.Person
        fields = ('phone_number',)

class BudgetRequestForm (forms.ModelForm):
    class Meta:
        model = afap.models.Application
        fields = ('balance_forward', 'afap_income',)

class AfapAuthenticationForm (auth.forms.AuthenticationForm):
    '''Authentication form that uses a selectable list of organizations in
    place of a username field'''
    username = forms.ModelChoiceField(label="Organization Name",
            queryset=afap.models.Organization.objects.exclude(user=None))
    def __init__(self, *args, **kwargs):
        super(AfapAuthenticationForm, self).__init__(*args, **kwargs)
        try:
            self.data['username'] = afap.models.Organization.objects.get(pk=self.data['username']).user.username
        except (ObjectDoesNotExist, KeyError):
            pass
        print 'authform username:'
        try:
            print self.data['username']
        except:
            pass

class OrganizationSelectForm (forms.Form):
    organization = forms.ModelChoiceField(
        queryset=afap.models.Organization.objects.all().order_by('name'))
