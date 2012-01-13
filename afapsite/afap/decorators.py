import afap.models
from django.core import urlresolvers
import afap.views
from django import shortcuts
from django.contrib import auth
from django.http import HttpRequest
from django.core.exceptions import ObjectDoesNotExist, MultipleObjectsReturned

def org_required(f):
    @auth.decorators.login_required
    def org_required_wrapper(request, *args, **kwargs):
        # if user is superuser, look at session and determine group, or redirect to
        # group selection
        if request.user.is_superuser:
            try:
                org = afap.models.Organization.objects.get(pk=request.session['organization'])
            except (ObjectDoesNotExist, KeyError, TypeError):
                return shortcuts.redirect(urlresolvers.reverse(afap.views.organization_select))
        else:
            try:
                org = afap.models.Organization.objects.get(user=request.user)
            except ObjectDoesNotExist:
                return shortcuts.redirect(urlresolvers.reverse(logout))
            if not org.active:
                # if organization is not active, de-link and log out user
                org.user = None
                org.save()
                return shortcuts.redirect(urlresolvers.reverse(afap.views.logout))
        request.session['organization'] = org.pk
        return f(request, org, *args, **kwargs)
    return org_required_wrapper

