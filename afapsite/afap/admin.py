import os
from afap import models
import afap.views
from django.contrib import admin
from django.forms import widgets
from django.contrib import auth
from django.contrib.auth.admin import UserAdmin
from django.core import urlresolvers
from django import shortcuts
from django.conf.urls.defaults import *

class AfapAdmin(admin.sites.AdminSite):
    def get_urls(self):
        urls = super(AfapAdmin, self).get_urls()
        return patterns('',
            (r'^back/$', self.back_to_home),
            url(r'^import_orgs/$', self.admin_view(self.import_orgs), name='import_orgs'),
            url(r'^kill_process/$', self.admin_view(self.kill_process), name='kill_process'),
        ) + urls

    def back_to_home(self, request):
        return shortcuts.redirect(urlresolvers.reverse(afap.views.home))

    def import_orgs(self, request):
        return shortcuts.redirect(urlresolvers.reverse('admin:index'))

    def kill_process(self, request):
        os.abort()

class LineItemInline(admin.TabularInline):
    model = models.LineItem
    extra = 1

class ApprovalInline(admin.TabularInline):
    model = models.Approval
    extra = 0
    fields = ('approver', 'approved', 'approved_at', 'key',)
    readonly_fields = ('approver', 'approved', 'approved_at', 'key',)

class FileInline(admin.TabularInline):
    model = models.ApplicationFile
    extra = 1

class AllocationAdmin(admin.ModelAdmin):
    model = models.Allocation
    extra = 1

class ApplicationAdmin(admin.ModelAdmin):
    model = models.Application
#    inlines = [LineItemInline, ApprovalInline, FileInline]
    inlines = [LineItemInline, FileInline]
    list_display = ('organization', '_year_for_admin', 'organization_constituency', 'provisional', 'has_budget_request')
    readonly_fields = ('last_modified',)
    ordering = ('organization',)
    search_fields = ('organization__name',)
    list_filter = ('organization__constituency_group__name',)

class OrganizationAdmin(admin.ModelAdmin):
    model = models.Organization
    ordering = ('name',)
    search_fields = ('name',)

class PersonAdmin(admin.ModelAdmin):
    model = models.Person
    ordering = ('name',)
    search_fields = ('name',)

class ApprovalAdmin(admin.ModelAdmin):
    model = models.Approval
    search_fields = ('application__organization__name',)

admin_site = AfapAdmin()
admin_site.register(models.Year)
admin_site.register(models.Person, PersonAdmin)
admin_site.register(models.ConstituencyGroup)
admin_site.register(models.Organization, OrganizationAdmin)
admin_site.register(models.Application, ApplicationAdmin)
admin_site.register(models.LineItemCategory)
admin_site.register(models.Approval, ApprovalAdmin)
admin_site.register(models.Allocation, AllocationAdmin)
admin_site.register(auth.models.User, UserAdmin)
#admin_site.register(models.LineItem)
#admin_site.register(models.Approval)
