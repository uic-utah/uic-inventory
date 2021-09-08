import Profile from './Profile';
import ContactProgram from './ContactProgram';

import CreateOrEditInventory from './Wells/CreateOrEditInventory';
import AddWells from './Wells/AddWells';

import CreateOrEditSite from './Sites/CreateOrEditSite';
import AddSiteContacts from './Sites/AddSiteContacts';
import AddSiteLocation from './Sites/AddSiteLocation';

const Sites = { CreateOrEditSite, AddSiteContacts, AddSiteLocation };
const Wells = { CreateOrEditInventory, AddWells };

export { SitesAndInventory, GenericLandingPage } from './Home';
export { ContactProgram, Profile, Sites, Wells };