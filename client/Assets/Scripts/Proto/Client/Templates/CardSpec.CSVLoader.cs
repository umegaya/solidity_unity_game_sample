#include "CardSpec.loader.h"
#include "LoaderConfig.h"
namespace mgo {
void LoadCardSpec(const std::string &filename, google::protobuf::Map<uint32_t, ::ch::CardSpec> &store, bool ischunk) {
	std::istringstream strm(filename);
	MyReader<6> *csv = ischunk ? new MyReader<6>("CardSpec", strm) : new MyReader<6>(filename);
	csv->read_header(io::ignore_extra_column, "issuance", "hp", "attack", "defense", "flags", "skills");
	uint32_t issuance;
	uint32_t hp;
	uint32_t attack;
	uint32_t defense;
	uint32_t flags;
	uint32_t skills;
	while(csv->read_row(issuance, hp, attack, defense, flags, skills)) {
		::ch::CardSpec row;
		row.set_issuance(issuance);
		row.set_hp(hp);
		row.set_attack(attack);
		row.set_defense(defense);
		row.set_flags(flags);
		row.set_skills(skills);

		store[row.id()] = row;
	}
	delete csv;
}
} //namespace mgo