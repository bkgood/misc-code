# encoding: utf-8
import datetime
from south.db import db
from south.v2 import SchemaMigration
from django.db import models

class Migration(SchemaMigration):

    def forwards(self, orm):
        
        # Deleting field 'Person.first_name'
        db.delete_column('afap_person', 'first_name')

        # Deleting field 'Person.last_name'
        db.delete_column('afap_person', 'last_name')

        # Adding field 'Person.name'
        db.add_column('afap_person', 'name', self.gf('django.db.models.fields.CharField')(default='NAME PLACEHOLDER', max_length=255), keep_default=False)


    def backwards(self, orm):
        
        # User chose to not deal with backwards NULL issues for 'Person.first_name'
        raise RuntimeError("Cannot reverse this migration. 'Person.first_name' and its values cannot be restored.")

        # User chose to not deal with backwards NULL issues for 'Person.last_name'
        raise RuntimeError("Cannot reverse this migration. 'Person.last_name' and its values cannot be restored.")

        # Deleting field 'Person.name'
        db.delete_column('afap_person', 'name')


    models = {
        'afap.application': {
            'Meta': {'object_name': 'Application'},
            'advisor': ('django.db.models.fields.related.ForeignKey', [], {'blank': 'True', 'related_name': "'advisor'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'dues': ('django.db.models.fields.DecimalField', [], {'default': "'0'", 'max_digits': '10', 'decimal_places': '2'}),
            'email_advisor': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'email_president': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'email_treasurer': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'last_modified': ('django.db.models.fields.DateTimeField', [], {'blank': 'True'}),
            'members': ('django.db.models.fields.IntegerField', [], {'default': '0'}),
            'membership_requirements': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'new_members': ('django.db.models.fields.IntegerField', [], {'default': '0'}),
            'note': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'organization': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Organization']"}),
            'president': ('django.db.models.fields.related.ForeignKey', [], {'blank': 'True', 'related_name': "'president'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'purpose': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'treasurer': ('django.db.models.fields.related.ForeignKey', [], {'blank': 'True', 'related_name': "'treasurer'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'year': ('afap.modelfields.YearField', [], {})
        },
        'afap.applicationfile': {
            'Meta': {'object_name': 'ApplicationFile'},
            'application': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Application']"}),
            'file': ('django.db.models.fields.files.FileField', [], {'max_length': '100'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'replaces_application': ('django.db.models.fields.BooleanField', [], {'default': 'True'})
        },
        'afap.approval': {
            'Meta': {'object_name': 'Approval'},
            'application': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Application']"}),
            'approved': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'approved_at': ('django.db.models.fields.DateTimeField', [], {'null': 'True', 'blank': 'True'}),
            'approver': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Person']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'key': ('django.db.models.fields.CharField', [], {'max_length': '40', 'blank': 'True'}),
            'notes': ('django.db.models.fields.TextField', [], {'blank': 'True'})
        },
        'afap.constituencygroup': {
            'Meta': {'object_name': 'ConstituencyGroup'},
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'})
        },
        'afap.lineitem': {
            'Meta': {'object_name': 'LineItem'},
            'amount': ('django.db.models.fields.DecimalField', [], {'max_digits': '10', 'decimal_places': '2'}),
            'application': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Application']"}),
            'category': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.LineItemCategory']"}),
            'description': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'income': ('django.db.models.fields.BooleanField', [], {'default': 'False'})
        },
        'afap.lineitemcategory': {
            'Meta': {'object_name': 'LineItemCategory'},
            'description': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'years': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['afap.Year']", 'symmetrical': 'False'})
        },
        'afap.organization': {
            'Meta': {'object_name': 'Organization'},
            'account_number': ('django.db.models.fields.CharField', [], {'max_length': '255', 'null': 'True', 'blank': 'True'}),
            'active': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'constituency_group': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.ConstituencyGroup']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'user': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['auth.User']", 'null': 'True', 'blank': 'True'})
        },
        'afap.person': {
            'Meta': {'object_name': 'Person'},
            'email_address': ('django.db.models.fields.EmailField', [], {'max_length': '75'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'phone_number': ('django.contrib.localflavor.us.models.PhoneNumberField', [], {'max_length': '20', 'blank': 'True'})
        },
        'afap.year': {
            'Meta': {'ordering': "['date']", 'object_name': 'Year'},
            'active': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'date': ('afap.modelfields.YearField', [], {'unique': 'True'}),
            'email_from': ('django.db.models.fields.EmailField', [], {'max_length': '75'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'read_only': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'validation_enabled': ('django.db.models.fields.BooleanField', [], {'default': 'True'})
        },
        'auth.group': {
            'Meta': {'object_name': 'Group'},
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'unique': 'True', 'max_length': '80'}),
            'permissions': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Permission']", 'symmetrical': 'False', 'blank': 'True'})
        },
        'auth.permission': {
            'Meta': {'ordering': "('content_type__app_label', 'content_type__model', 'codename')", 'unique_together': "(('content_type', 'codename'),)", 'object_name': 'Permission'},
            'codename': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'content_type': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['contenttypes.ContentType']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '50'})
        },
        'auth.user': {
            'Meta': {'object_name': 'User'},
            'date_joined': ('django.db.models.fields.DateTimeField', [], {'default': 'datetime.datetime.now'}),
            'email': ('django.db.models.fields.EmailField', [], {'max_length': '75', 'blank': 'True'}),
            'first_name': ('django.db.models.fields.CharField', [], {'max_length': '30', 'blank': 'True'}),
            'groups': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Group']", 'symmetrical': 'False', 'blank': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'is_active': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'is_staff': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'is_superuser': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'last_login': ('django.db.models.fields.DateTimeField', [], {'default': 'datetime.datetime.now'}),
            'last_name': ('django.db.models.fields.CharField', [], {'max_length': '30', 'blank': 'True'}),
            'password': ('django.db.models.fields.CharField', [], {'max_length': '128'}),
            'user_permissions': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Permission']", 'symmetrical': 'False', 'blank': 'True'}),
            'username': ('django.db.models.fields.CharField', [], {'unique': 'True', 'max_length': '30'})
        },
        'contenttypes.contenttype': {
            'Meta': {'ordering': "('name',)", 'unique_together': "(('app_label', 'model'),)", 'object_name': 'ContentType', 'db_table': "'django_content_type'"},
            'app_label': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'model': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '100'})
        }
    }

    complete_apps = ['afap']
