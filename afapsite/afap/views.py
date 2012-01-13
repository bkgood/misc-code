import os.path
import mimetypes

from django import shortcuts
from django import http
from django.contrib import auth
import django.contrib.auth.views
import django.template
from django.template import Context, Template
from django.template.loader import render_to_string, get_template
from django.core import urlresolvers
from django.core.exceptions import ObjectDoesNotExist, MultipleObjectsReturned
from django import template
from django.forms.models import modelformset_factory
from django.db.models import Sum
from django.contrib import messages
from django.http import HttpResponse

import afap.forms
import afap.models
import afap.decorators
from afap.util.rangeddate import RangedDate
from afap.util import redirect_to_application, sum_line_items

@afap.decorators.org_required
def home(request, org):
    active_year = afap.models.Year.objects.get(active=True)
    has_application = True
    try:
        app = afap.models.Application.objects.get(organization=org, year=active_year.date)
    except (afap.models.Application.DoesNotExist):
        has_application = False
    return shortcuts.render_to_response('home.html', {
        'application_list': afap.models.Application.objects.filter(organization=org),
        'active_year': active_year, 'user': request.user, 'has_application': has_application,
        'current_organization': org,
        }, template.RequestContext(request))

def login(request):
    return auth.views.login(request, "login.html")

@auth.decorators.login_required
def logout(request):
    return auth.views.logout(request, next_page=urlresolvers.reverse(home))

@auth.decorators.login_required
def password_change(request):
    return auth.views.password_change(request, "password_change.html", "/")

@afap.decorators.org_required
def application(request, org, year):
    try:
        year = afap.models.Year.objects.get(date=RangedDate(int(year), 1, 1))
        app = afap.models.Application.objects.get(organization=org, year=year.date)
    except (ValueError, afap.models.Year.DoesNotExist):
        return shortcuts.redirect(urlresolvers.reverse(home))
    except afap.models.Application.DoesNotExist:
        messages.error(request, "You do not have an application for year %s" % year.range)
        return shortcuts.redirect(urlresolvers.reverse(home))
    files = app.applicationfile_set.filter(replaces_application=True)
    if files:
        f = files[0]
        f.file.open()
        resp = HttpResponse(f.file.read(),
            mimetype=mimetypes.guess_type(f.file.path) or 'application/octet-stream')
        f.file.close()
        resp['Content-Disposition'] = "attachment; filename=%s" % (os.path.basename(f.file.name),)
        return resp
    if request.method == 'POST':
        if year.read_only:
            return shortcuts.redirect(urlresolvers.reverse(application,
                args=(year.year,)))
        form = afap.forms.ApplicationForm(request.POST, instance=app, prefix="app")
        pres_form = afap.forms.PhoneNumberForm(request.POST, instance=app.president, prefix="president")
        treas_form = afap.forms.PhoneNumberForm(request.POST, instance=app.treasurer, prefix="treasurer")
        if form.is_valid():
            form.save()
        weHaveAPhoneNumber = False
        if pres_form.is_valid():
            if pres_form.cleaned_data['phone_number'] != "":
                weHaveAPhoneNumber = True
        if treas_form.is_valid():
            if treas_form.cleaned_data['phone_number'] != "":
                weHaveAPhoneNumber = True
        if weHaveAPhoneNumber:
            if pres_form.is_valid():
                pres_form.save()
            if treas_form.is_valid():
                treas_form.save()
        else:
            messages.error(request, "Either the president or the treasurer must have a phone number.")
        if weHaveAPhoneNumber and form.is_valid() and pres_form.is_valid() and treas_form.is_valid():
            messages.info(request, "Your changes have been saved.")
            return shortcuts.redirect(urlresolvers.reverse(application, args=(year.year,)))
    else:
        form = afap.forms.ApplicationForm(instance=app, prefix="app")
        pres_form = afap.forms.PhoneNumberForm(instance=app.president, prefix="president")
        treas_form = afap.forms.PhoneNumberForm(instance=app.treasurer, prefix="treasurer")
        adv_form = afap.forms.PhoneNumberForm(instance=app.advisor, prefix="advisor")
    return shortcuts.render_to_response('application.html', {'form': form, 'year': year,
        'active_year': year, 'read_only': year.read_only, 'application': app, 'pres_form': pres_form,
        'treas_form': treas_form,},
        template.RequestContext(request))

@afap.decorators.org_required
def budgetrequest(request, org, year):
    try:
        year = afap.models.Year.objects.get(date=RangedDate(int(year), 1, 1))
        app = afap.models.Application.objects.get(organization=org, year=year.date)
    except ValueError:
        return shortcuts.redirect(urlresolvers.reverse(home))
    except afap.models.Application.DoesNotExist:
        messages.error(request, "You do not have an application for year %s" % year.range)
        return shortcuts.redirect(urlresolvers.reverse(home))
    if redirect_to_application(app):
        return shortcuts.redirect(urlresolvers.reverse(application, args=(year.year,)))
    if request.method == 'POST':
        if year.read_only:
            return shortcuts.redirect(urlresolvers.reverse(budgetrequest,
                args=(year.year,)))
        form = afap.forms.BudgetRequestForm(request.POST, instance=app)
        if form.is_valid():
            form.save()
            messages.info(request, "Your changes have been saved.")
            return shortcuts.redirect(urlresolvers.reverse(budgetrequest, args=(year.year,)))
    else:
        form = afap.forms.BudgetRequestForm(instance=app)
    categories = []
    current_year = {}
    # XXX HACKS -- make current year categories always come first!
    for category in afap.models.LineItemCategory.objects.filter(years__date=year.date).order_by('pk'):
        category_sum = sum_line_items(
            afap.models.LineItem.objects.filter(application=app, category=category))
        # XXX HACKS
        if category.name.startswith("Current"):
            current_year['name'] = category.name
            current_year['sum'] = category_sum
            current_year['pk'] = category.pk
            current_year['end_balance'] = category_sum - app.balance_forward - app.afap_income
        else:
            categories.append({'name': category.name, 'sum': category_sum, 'pk': category.pk})
    total_sum = sum([x['sum'] for x in categories]) + current_year['end_balance']
    total_sum = total_sum or 0
    total_request = max(total_sum, 0)

    return shortcuts.render_to_response('budgetrequest.html', {
        'categories': categories, 'total_sum': total_sum, 'year': year, 'active_year': year,
        'total_request': total_request, 'form': form, 'current_year': current_year,
        'read_only': year.read_only},
        template.RequestContext(request))

@afap.decorators.org_required
def budgetrequestcategory(request, org, year, category):
    LineItemFormSet = modelformset_factory(afap.models.LineItem,
        exclude=('application', 'category'), can_delete=True, extra=10)
    try:
        year = afap.models.Year.objects.get(date=RangedDate(int(year), 1, 1))
    except (ValueError, afap.models.Year.DoesNotExist):
        return shortcuts.redirect(urlresolvers.reverse(home))
    try:
        app = afap.models.Application.objects.get(organization=org, year=year.date)
        category = afap.models.LineItemCategory.objects.get(pk=category, years=year)
    except afap.models.Application.DoesNotExist:
        messages.error(request, "You do not have an application for year %s" % year.range)
        return shortcuts.redirect(urlresolvers.reverse(home))
    except afap.models.LineItemCategory.DoesNotExist:
        return shortcuts.redirect(urlresolvers.reverse(budgetrequest, args=(year.year,)))
    if redirect_to_application(app):
        return shortcuts.redirect(urlresolvers.reverse(application, args=(year.year,)))
    if request.method == 'POST':
        if year.read_only:
            return shortcuts.redirect(urlresolvers.reverse(budgetrequestcategory,
                args=[year.year, category.pk]))
        formset = LineItemFormSet(request.POST)
        if formset.is_valid():
            instances = formset.save(commit=False)
            for instance in instances:
                instance.application = app
                instance.category = category
                instance.save()
            messages.info(request, "Your changes have been saved.")
            return shortcuts.redirect(urlresolvers.reverse(budgetrequestcategory,
                args=[year.year, category.pk]))
    else:
        formset = LineItemFormSet(
            queryset=afap.models.LineItem.objects.filter(application=app, category=category))
    for form in formset.forms:
        form.fields['description'].widget.attrs['class'] = 'itemdescription'
        form.fields['amount'].widget.attrs['class'] = 'itemamount'
        form.fields['income'].widget.attrs['class'] = 'itemincome'
        form.fields['DELETE'].widget.attrs['class'] = 'itemdelete'
    category_sum = sum_line_items(
        afap.models.LineItem.objects.filter(application=app, category=category))
    return shortcuts.render_to_response('budgetrequestcategory.html', {
        'formset': formset, 'year': year, 'active_year': year, 'category': category,
        'category_sum': category_sum, 'read_only': year.read_only,},
        template.RequestContext(request))

@afap.decorators.org_required
def signatures(request, org, year):
    try:
        year = afap.models.Year.objects.get(date=RangedDate(int(year), 1, 1))
        app = afap.models.Application.objects.get(organization=org, year=year.date)
    except (ValueError, afap.models.Year.DoesNotExist):
        return shortcuts.redirect(urlresolvers.reverse(home))
    except afap.models.Application.DoesNotExist:
        messages.error(request, "You do not have an application for year %s" % year.range)
        return shortcuts.redirect(urlresolvers.reverse(home))
    if redirect_to_application(app):
        return shortcuts.redirect(urlresolvers.reverse(application, args=(year.year,)))
    if request.method == 'POST':
        people = (app.president, app.treasurer, app.advisor)
        for approval in app.approval_set.filter(approver__in=people):
            t = get_template('emails/approve.txt')
            c = Context({'to': approval.approver.name, 'organization': approval.application.organization.name,
                'location': request.build_absolute_uri(urlresolvers.reverse(approve, args=(year.year, approval.key)))})
            from_ = afap.models.Year.objects.get(date=approval.application.year).email_from
            # can we only email when the approver hasn't yet approved the application?
            approval.approver.email("AFAP Application Approval", t.render(c), from_)
        return shortcuts.redirect(urlresolvers.reverse(signatures, args=(year.year,)))
    return shortcuts.render_to_response('signatures.html', {'last_modified': app.last_modified,
        'approvals': app.approval_set.all(), 'year': year, 'active_year': year,},
        template.RequestContext(request))

@afap.decorators.org_required
def archive(request, org):
    active_year = afap.models.Year.objects.get(active=True)
    archived_years = afap.models.Application.objects.filter(organization=org).exclude(
        year=active_year.year).values_list('year', flat=True)
    archived_years = [RangedDate(archived_year) for archived_year in archived_years]
    return shortcuts.render_to_response('archive.html', {
        'active_year': active_year, 'archived_years': archived_years},
        template.RequestContext(request))

def approve(request, year, hash):
    try:
        year = afap.models.Year.objects.get(date=RangedDate(int(year), 1, 1))
        approval = afap.models.Approval.objects.get(key=hash)
    except (ValueError, afap.models.Year.DoesNotExist, afap.models.Approval.DoesNotExist):
        messages.error(request, "Invalid approval link; please check your email account "
            + "for an updated link or contact the person in your organization responsible "
            + "for the AFAP application.")
        return shortcuts.render_to_response('approval.html', {},
            template.RequestContext(request))
    if request.method == 'POST':
        form = afap.forms.ApprovalForm(request.POST, instance=approval)
        if form.is_valid():
            form.save()
            messages.info(request, "Your changes have been saved.")
            return shortcuts.redirect(urlresolvers.reverse(afap.views.approve,
                args=(year.year, hash,)))
        else:
            messages.error(request, "An error occurred in saving your changes, "
                + "please refer to any error messages below.")
    app = afap.forms.ApplicationForm(instance=approval.application)
    budget_request_by_cat = {}
    line_items = afap.models.LineItem.objects.filter(application=approval.application)
    for line_item in line_items:
        try:
            budget_request_by_cat[line_item.category]['line_items'].append(line_item)
        except (KeyError):
            budget_request_by_cat[line_item.category] = {'line_items': [line_item,],}
    budget_request = []
    for category in budget_request_by_cat:
        budget_request.append({
            'category': category,
            'line_items': budget_request_by_cat[category]['line_items'],
            'sum': sum_line_items(budget_request_by_cat[category]['line_items']),
        })
    total_sum = sum([x['sum'] for x in budget_request]) - (
        approval.application.balance_forward + approval.application.afap_income)
    total_request = max(total_sum, 0)
    form = afap.forms.ApprovalForm(instance=approval)
    return shortcuts.render_to_response('approval.html', {
        'form': form, 'application': app, 'budget_request': budget_request, 'approval': approval,
        'total_sum': total_sum, 'total_request': total_request, 'year': year, 'hash': hash,
        'balance_forward': approval.application.balance_forward,
        'afap_income': approval.application.afap_income,},
        template.RequestContext(request))

@afap.decorators.org_required
def contact(request, org):
    active_year = afap.models.Year.objects.get(active=True)
    app = shortcuts.get_object_or_404(afap.models.Application.objects, organization=org,
        year=active_year.year)
    contacts = [
        {'title': 'President', 'person': app.president},
        {'title': 'Treasurer', 'person': app.treasurer},
        {'title': 'Advisor', 'person': app.advisor},
    ]
    if request.method == 'POST':
        form = afap.forms.ContactForm(request.POST, instance=app)
        if form.is_valid():
            form.save()
            messages.info(request, "Your changes have been saved.")
            return shortcuts.redirect(urlresolvers.reverse(contact))
    else:
        form = afap.forms.ContactForm(instance=app)
    return shortcuts.render_to_response('contact.html', {'contacts': contacts, 'form': form,
        }, template.RequestContext(request))

@auth.decorators.login_required
def organization_select(request):
    if not request.user.is_superuser:
        return shortcuts.redirect(urlresolvers.reverse(home))
    if request.method == 'POST':
        form = afap.forms.OrganizationSelectForm(request.POST)
        if form.is_valid():
            request.session['organization'] = form.cleaned_data['organization'].pk
            return shortcuts.redirect(urlresolvers.reverse(home))
    form = afap.forms.OrganizationSelectForm()
    active_year = afap.models.Year.objects.get(active=True)
    return shortcuts.render_to_response('organization_select.html',
        {'active_year': active_year, 'form': form,
        }, template.RequestContext(request))

